# Base image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Cloud Run listens on port 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Final image
FROM base AS final
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "RAGServer.API.dll"]
