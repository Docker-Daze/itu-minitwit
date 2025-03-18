# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /minitwit

# Copy only project files first
COPY . ./

# Restore dependencies
RUN dotnet restore

# Build and publish
WORKDIR /minitwit/src/minitwit.web
RUN dotnet publish -c Release -o /out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /minitwit
COPY --from=build /out .

EXPOSE 5000
