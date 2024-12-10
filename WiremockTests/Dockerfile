# Use the official .NET image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app

# Use the official .NET image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["MockOktaApi.csproj", "./"]
RUN dotnet restore "./MockOktaApi.csproj"

# Copy the rest of the application files
COPY . .
WORKDIR "/src/"
RUN dotnet build "MockOktaApi.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MockOktaApi.csproj" -c Release -o /app/publish

# Final stage: runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MockOktaApi.dll"]