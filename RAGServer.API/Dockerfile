# Base image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Copy service-account.json
COPY service-account.json /app/service-account.json
ENV GOOGLE_APPLICATION_CREDENTIALS="/app/service-account.json"

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RAGServer.API.dll"]
