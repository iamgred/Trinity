
FROM python:3.12-slim

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY redirector.py .

# Docker-internal application port; Compose publishes it on host loopback.
EXPOSE 5000

# Flask binds to 0.0.0.0 in redirector.py so this container port is reachable
# from the Compose network and from the configured host publication.
CMD ["python", "redirector.py"]