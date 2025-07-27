timestamp=$(date '+%Y%m%d%H%M%S')

docker buildx build --platform linux/amd64 -t rafaelgiori/rinha-dotnet:amd64-$timestamp .
docker push rafaelgiori/rinha-dotnet:amd64-$timestamp

docker buildx build --platform linux/arm64 -t rafaelgiori/rinha-dotnet:arm64-$timestamp .
docker push rafaelgiori/rinha-dotnet:arm64-$timestamp