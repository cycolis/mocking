FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Okta.Mobile/Okta.Mobile.csproj", "Okta.Mobile/"]
COPY ["src/Okta.DeviceAccess.Core/Okta.DeviceAccess.Core.csproj", "Okta.DeviceAccess.Core/"]

RUN dotnet restore "Okta.Mobile/Okta.Mobile.csproj"

COPY ["src/Okta.DeviceAccess.Core", "Okta.DeviceAccess.Core"]
COPY ["src/Okta.Mobile", "Okta.Mobile"]
WORKDIR "/src/Okta.Mobile"
RUN dotnet build "Okta.Mobile.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Okta.Mobile.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Okta.Mobile.dll"]