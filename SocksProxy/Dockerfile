FROM alpine:3.20

RUN apk add --no-cache socat

# Container-only listener consumed by redirector.py as socks5h://socks-proxy:1080.
EXPOSE 1080

# Bridge TCP clients to Tor's SOCKS listener. The service name "tor" is
# resolved by Docker's internal DNS; it is not a host-network address.
HEALTHCHECK --interval=5s --timeout=3s --retries=10 \
    CMD ["sh", "-c", "nc -z tor 9050"]

CMD ["socat", "TCP-LISTEN:1080,fork,reuseaddr,bind=0.0.0.0", "TCP:tor:9050"]
