FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app


FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["Syslog.Server/Syslog.Server.csproj", "Syslog.Server/"]
RUN dotnet restore "Syslog.Server/Syslog.Server.csproj"
COPY . .
WORKDIR "/src/Syslog.Server"
RUN dotnet build "Syslog.Server.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Syslog.Server.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Syslog.Server.dll","--AzureStorage_StorageConnectionString"]