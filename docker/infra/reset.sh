#!/bin/bash
echo "Stopping and removing all containers and volumes..."
docker compose down -v
docker compose up -d