FROM debian:bookworm-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends socat supervisor tor \
    && rm -rf /var/lib/apt/lists/*

COPY torrc /etc/tor/torrc
COPY supervisord.conf /etc/supervisor/conf.d/tor.conf

RUN mkdir -p /var/lib/tor/weather_api_hidden_service \
    && chown -R debian-tor:debian-tor /var/lib/tor \
    && chmod 700 /var/lib/tor/weather_api_hidden_service

VOLUME ["/var/lib/tor"]

EXPOSE 9050

HEALTHCHECK --interval=5s --timeout=3s --retries=20 \ CMD ["sh", "-c", "test -s /var/lib/tor/weather_api_hidden_service/hostname"]

CMD ["supervisord", "--nodaemon", "--configuration", "/etc/supervisor/supervisord.conf"]