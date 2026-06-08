FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EventHub.Api.Tests/EventHub.Api.Tests.csproj", "EventHub.Api.Tests/"]
COPY ["EventHub.Api/EventHub.Api.csproj", "EventHub.Api/"]
RUN dotnet restore "EventHub.Api.Tests/EventHub.Api.Tests.csproj"
COPY . .
WORKDIR "/src/EventHub.Api.Tests"
RUN dotnet build "./EventHub.Api.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EventHub.Api.Tests.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventHub.Api.Tests.dll"]
