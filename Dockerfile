# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["FritzApp/FritzApp.csproj", "FritzApp/"]
RUN dotnet restore "FritzApp/FritzApp.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/FritzApp"
RUN dotnet build "FritzApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FritzApp.csproj" -c Release -o /app/publish

# Runtime stage - serve with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy published output to nginx html directory
COPY --from=publish /app/publish/wwwroot .

# Copy nginx configuration
COPY FritzApp/nginx.config /etc/nginx/nginx.conf

EXPOSE 80
