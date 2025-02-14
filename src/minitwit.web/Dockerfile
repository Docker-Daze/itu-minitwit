
# Build runtime image
# FROM mcr.microsoft.com/dotnet/ 
# aspnet:9.0@sha256:b4bea3a52a0a77317fa93c5bbdb076623f81e3e2f201078d89914da71318b5d8
# WORKDIR /minitwit.web
# COPY --from=build /App/out .
# ENTRYPOINT ["dotnet", "minitwit.web.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /minitwit.web
COPY bin/Debug/Release ./
ENTRYPOINT ["dotnet", "minitwit.web.dll"]