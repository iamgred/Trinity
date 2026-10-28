
import os

from flask import Flask, jsonify
import requests

app = Flask(__name__)

# These values define the redirector's outbound communication contract.
# WEATHER_API_BASE is optional because the built-in route discovers the onion
# address from Tor's shared volume. WEATHER_API_PATH identifies the upstream
# application endpoint and WEATHER_API_PROXY selects the network hop used to
# reach it.
WEATHER_API_BASE = os.getenv("WEATHER_API_BASE", "").rstrip("/")
WEATHER_API_PATH = os.getenv("WEATHER_API_PATH", "/weatherforecast")
WEATHER_API_PROXY = os.getenv("WEATHER_API_PROXY", "socks5h://socks-proxy:1080").strip() or None
# This path must match HiddenServiceDir in Tor/torrc and the volume mount in
# compose.yaml when the API is exposed as an onion service.
ONION_HOSTNAME_FILE = os.getenv(
    "ONION_HOSTNAME_FILE",
    "/var/lib/tor/weather_api_hidden_service/hostname"
)


def get_weather_api_base():
    """Resolve the upstream origin without changing the configured route."""
    if WEATHER_API_BASE:
        return WEATHER_API_BASE

    try:
        with open(ONION_HOSTNAME_FILE, encoding="utf-8") as hostname_file:
            hostname = hostname_file.read().strip()
    except OSError:
        app.logger.exception("Unable to read the weather API onion hostname")
        raise RuntimeError("Weather API onion hostname is unavailable") from None

    if not hostname:
        raise RuntimeError("Weather API onion hostname is empty")

    return f"http://{hostname}"


def make_upstream_request(url, method="GET", data=None, params=None, proxy=None):
    """
    Send a request to an upstream API, optionally through a proxy.

    A proxy URL such as socks5h://tor:9050 is used for onion services.
    Leaving proxy unset keeps requests to the Docker-internal API direct.
    """
    # requests uses the proxy for both HTTP and HTTPS. The socks5h scheme is
    # important for .onion hosts because hostname resolution must happen via
    # Tor, not through the container's normal DNS resolver.
    headers = {
        "User-Agent": "project-redirector"
    }
    proxies = {"http": proxy, "https": proxy} if proxy else None

    try:
        method = method.upper()

        if method == "GET":
            response = requests.get(
                url,
                params=params,
                headers=headers,
                proxies=proxies,
                timeout=30
            )

        elif method == "POST":
            response = requests.post(
                url,
                json=data,
                params=params,
                headers=headers,
                proxies=proxies,
                timeout=30
            )

        else:
            return None, "Unsupported HTTP method"

        return response, None

    except requests.exceptions.RequestException:
        app.logger.exception("Upstream request failed")
        return None, "Upstream service unavailable"


@app.route("/health", methods=["GET"])
def health_check():
    # This endpoint is local service health, not an upstream connectivity test.
    return jsonify({
        "status": "online",
        "service": "redirector"
    }), 200


@app.route("/weather", methods=["GET"])
def weather():
    """Forward the client request across the configured upstream network path."""
    try:
        url = f"{get_weather_api_base()}/{WEATHER_API_PATH.lstrip('/')}"
    except RuntimeError as error:
        return jsonify({"error": str(error)}), 503

    response, error = make_upstream_request(
        url,
        method="GET",
        proxy=WEATHER_API_PROXY
    )

    if error:
        return jsonify({"error": error}), 502

    try:
        return jsonify(response.json()), response.status_code
    except ValueError:
        return jsonify({
            "error": "Upstream returned an invalid JSON response"
        }), 502


# ============================================================
# APPLICATION ENTRY POINT
# ============================================================

if __name__ == "__main__":
    # Flask binds to all container interfaces so Docker can reach this
    # process. Docker publishes only host loopback:5000 (see compose.yaml).
    # For deployment, use a production WSGI server with the same bind port.
    app.run(host="0.0.0.0", port=5000, debug=False)