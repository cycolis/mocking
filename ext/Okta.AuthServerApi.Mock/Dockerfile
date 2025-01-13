FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ext/Okta.AuthServerApi.Mock/Okta.AuthServerApi.Mock.csproj", "Okta.AuthServerApi.Mock/"]
COPY ["src/Okta.DeviceAccess.Core/Okta.DeviceAccess.Core.csproj", "Okta.DeviceAccess.Core/"]

RUN dotnet restore "Okta.AuthServerApi.Mock/Okta.AuthServerApi.Mock.csproj"

COPY ["ext/Okta.AuthServerApi.Mock", "Okta.AuthServerApi.Mock"]
COPY ["src/Okta.DeviceAccess.Core", "Okta.DeviceAccess.Core"]
WORKDIR "/src/Okta.AuthServerApi.Mock"
RUN dotnet build "Okta.AuthServerApi.Mock.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Okta.AuthServerApi.Mock.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ["ext/Okta.AuthServerApi.Mock/appsettings.Docker.json", "./"]
ENTRYPOINT ["dotnet", "Okta.AuthServerApi.Mock.dll"]