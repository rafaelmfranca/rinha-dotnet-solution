#!/bin/bash
timestamp=$(date '+%Y%m%d%H%M%S')

docker buildx build \
  --platform linux/amd64,linux/arm64 \
  -t rafaelgiori/rinha-dotnet:$timestamp \
  --push .