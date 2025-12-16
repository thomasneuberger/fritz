# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Fritz.sln", "./"]
COPY ["src/Fritz.Api/Fritz.Api.csproj", "src/Fritz.Api/"]
COPY ["src/Fritz.Shared/Fritz.Shared.csproj", "src/Fritz.Shared/"]

# Restore dependencies
RUN dotnet restore "src/Fritz.Api/Fritz.Api.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/src/Fritz.Api"
RUN dotnet build "Fritz.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Fritz.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fritz.Api.dll"]
