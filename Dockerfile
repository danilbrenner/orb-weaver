FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy Directory.Build.props and Directory.Packages.props for centralized package management
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

# Copy project files
COPY ["src/OrbWeaver.Domain/OrbWeaver.Domain.csproj", "src/OrbWeaver.Domain/"]
COPY ["src/OrbWeaver.Application/OrbWeaver.Application.csproj", "src/OrbWeaver.Application/"]
COPY ["src/OrbWeaver.Infrastructure/OrbWeaver.Infrastructure.csproj", "src/OrbWeaver.Infrastructure/"]
COPY ["src/OrbWeaver.Host/OrbWeaver.Host.csproj", "src/OrbWeaver.Host/"]

# Restore dependencies
RUN dotnet restore "src/OrbWeaver.Host/OrbWeaver.Host.csproj"

# Copy source code
COPY ["src/", "src/"]

# Build
WORKDIR "/src/src/OrbWeaver.Host"
RUN dotnet build "OrbWeaver.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OrbWeaver.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrbWeaver.Host.dll"]

