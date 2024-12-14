FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Okta.AuthServerApi/Okta.AuthServerApi.csproj", "Okta.AuthServerApi/"]
COPY ["src/Okta.DeviceAccess.Core/Okta.DeviceAccess.Core.csproj", "Okta.DeviceAccess.Core/"]

RUN dotnet restore "Okta.AuthServerApi/Okta.AuthServerApi.csproj"

COPY ["src/Okta.DeviceAccess.Core", "Okta.DeviceAccess.Core"]
COPY ["src/Okta.AuthServerApi", "Okta.AuthServerApi"]
WORKDIR "/src/Okta.AuthServerApi"
RUN dotnet build "Okta.AuthServerApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Okta.AuthServerApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Okta.AuthServerApi.dll"]