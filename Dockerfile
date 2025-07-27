FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG TARGETPLATFORM

RUN apk add --no-cache clang build-base zlib-dev

WORKDIR /src
COPY . .

RUN dotnet restore --verbosity detailed
RUN dotnet publish src/Api/Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS runtime

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["./Api"]