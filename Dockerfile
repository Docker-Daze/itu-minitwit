# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /minitwit

COPY *.sln ./

# Copy only project files first (to improve caching)
COPY src/minitwit.core/ ./src/minitwit.core/
COPY src/minitwit.infrastructure/ ./src/minitwit.infrastructure/
COPY src/minitwit.web/ ./src/minitwit.web/
COPY tests/minitwit.tests/ tests/minitwit.tests/

# Restore dependencies (cached layer)
RUN dotnet restore itu-minitwit.sln

# Build and publish
WORKDIR /minitwit/src/minitwit.web
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /minitwit
COPY --from=build /out .

# Expose port 5000 (assuming your app runs here)
EXPOSE 5000

# Run the application
ENTRYPOINT ["dotnet", "minitwit.web.dll"]