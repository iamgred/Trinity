# Host the ASP.NET Core WeatherForecast API on a .onion Address

The project uses the ASP.NET Core `WeatherForecastApi` application instead of
the previous PHP API. The API uses HTTP only, listens on port `8080` in Docker,
and exposes `GET /weatherforecast`.

## Run the API with Docker Compose

From the repository root:

```bash
docker compose up --build tor weather-api redirector
```

This starts four containers: `tor`, `socks-proxy`, `weather-api`, and
`redirector`. The redirector is available at `http://127.0.0.1:5000`. Verify
the complete path through the redirector:

```bash
curl http://127.0.0.1:5000/health
curl http://127.0.0.1:5000/weather
```

The redirector reaches the API through the generated `.onion` hostname and the
internal SOCKS5 proxy. Direct API access is also available at
`http://127.0.0.1:8080`.

The dedicated `socks-proxy` container listens on port `1080` and forwards
SOCKS5 connections to the Tor container's port `9050`. It is intentionally
internal to the Compose network.

## Communication and port map

The deployment has several different kinds of ports. A **container port** is
reachable by services on the `onion-internal` Docker network; a **published
host port** is reachable from the Docker host; an **onion port** is reachable
through Tor. They are not interchangeable.

| Hop | Listener | Destination | Purpose |
| --- | --- | --- | --- |
| Local client -> redirector | host `127.0.0.1:5000` | redirector `:5000` | Client entry point |
| Redirector -> SOCKS relay | `socks-proxy:1080` | Tor `:9050` | Internal SOCKS5 transport |
| Tor onion -> relay | Tor `:80` | Tor loopback `:18080` | Hidden-service HTTP port |
| Tor relay -> API | Tor `:18080` | `weather-api:8080` | Internal HTTP bridge |
| Local client -> API | host `127.0.0.1:8080` | API `:8080` | Optional direct diagnostics |

When replacing the API, update the API listener (`ASPNETCORE_URLS`), the
`weather-api` health check, the `TCP:weather-api:8080` destination in
`Tor/supervisord.conf`, and the `HiddenServicePort` target chain in
`Tor/torrc`. When changing a client-facing port, update the corresponding
Compose `ports` entry and all curl/client URLs. Internal ports should remain
unpublished unless direct host access is explicitly required.

## Reconfiguring this for another application

1. Change `WEATHER_API_PATH` to the new application route, or rename the
   environment variable and its use in `Redirector/redirector.py` if the
   upstream is no longer a weather API.
2. Keep the upstream base URL free of a trailing slash. Use
   `WEATHER_API_BASE` for a fixed HTTP origin; leave it empty to discover the
   hostname in the Tor volume.
3. Keep `socks5h://` when addressing `.onion` hosts so DNS resolution occurs
   through Tor. Use no proxy only when the upstream is directly reachable on
   the Docker network.
4. If the application changes protocol, replace the HTTP request code in the
   redirector and the `socat` TCP bridge together. The current path assumes
   HTTP over TCP at every application hop.
5. Keep health checks aligned with the new service's actual local path and
   port. A healthy Docker container does not necessarily mean the onion route
   is healthy.

## Route the API through a proxy

If the API is hosted at an HTTP address reachable through a SOCKS proxy, set
both values when starting Compose:

```bash
WEATHER_API_BASE=http://your-api-address \
WEATHER_API_PROXY=socks5h://socks-proxy:1080 \
docker compose up --build tor redirector
```

For the built-in onion service, leave `WEATHER_API_BASE` empty. The redirector
reads the generated hostname from the shared Tor volume and uses
`socks5h://socks-proxy:1080`.

## Host the API as a Tor onion service

Install Tor on the host:

```bash
sudo apt update
sudo apt install tor -y
```

Edit `/etc/tor/torrc` and add an HTTP-only hidden-service mapping:

```
HiddenServiceDir /var/lib/tor/myapi_hidden_service/
HiddenServicePort 80 127.0.0.1:8080
```

Publish the API port by adding this mapping to the `weather-api` service in
`compose.yaml`:

```yaml
ports:
  - "127.0.0.1:8080:8080"
```

Restart Tor and get the onion address:

```bash
sudo systemctl restart tor
sudo cat /var/lib/tor/myapi_hidden_service/hostname
```

Set the resulting address in the redirector:

```bash
WEATHER_API_BASE=http://YOUR_ONION_ADDRESS \
WEATHER_API_PROXY=socks5h://socks-proxy:1080 \
docker compose up --build tor redirector
```

Use the [Tor Browser](https://www.torproject.org/) or the redirector's
`/weather` endpoint to access the onion service.
