#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

# Build ServiceStack libraries
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS ss-build
WORKDIR /src
ARG SS_VERSION=v5.10.4
RUN git clone --depth 1 --branch ${SS_VERSION} https://github.com/ServiceStack/ServiceStack ./ServiceStack
RUN git clone --depth 1 --branch ${SS_VERSION} https://github.com/ServiceStack/ServiceStack.OrmLite ./ServiceStack.OrmLite
RUN git clone --depth 1 --branch ${SS_VERSION} https://github.com/ServiceStack/ServiceStack.Redis ./ServiceStack.Redis
RUN git clone --depth 1 --branch ${SS_VERSION} https://github.com/ServiceStack/ServiceStack.Text ./ServiceStack.Text

RUN dotnet publish ./ServiceStack.OrmLite/src/ServiceStack.OrmLite.Sqlite/ServiceStack.OrmLite.Sqlite.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack.OrmLite/src/ServiceStack.OrmLite.SqlServer/ServiceStack.OrmLite.SqlServer.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack.OrmLite/src/ServiceStack.OrmLite.PostgreSQL/ServiceStack.OrmLite.PostgreSQL.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack.OrmLite/src/ServiceStack.OrmLite.MySqlConnector/ServiceStack.OrmLite.MySqlConnector.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack.Redis/src/ServiceStack.Redis/ServiceStack.Redis.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack.Text/src/ServiceStack.Text/ServiceStack.Text.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack/src/ServiceStack/ServiceStack.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack/src/ServiceStack.RabbitMq/ServiceStack.RabbitMq.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack/src/ServiceStack.Server/ServiceStack.Server.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack/src/ServiceStack.Kestrel/ServiceStack.Kestrel.csproj -c Release -f netstandard2.0 -o /lib
RUN dotnet publish ./ServiceStack/src/ServiceStack.Mvc/ServiceStack.Mvc.csproj -c Release -f netstandard2.0 -o /lib

FROM scratch AS ss-lib
COPY --from=ss-build /lib /lib/ServiceStack

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY --from=ss-lib /lib/ServiceStack ./lib/ServiceStack
#COPY ["nuget.config", "."]
COPY ["Ekklesia.Api/Ekklesia.Api.csproj", "Ekklesia.Api/"]
COPY ["Ekklesia.Api.ServiceInterface/Ekklesia.Api.ServiceInterface.csproj", "Ekklesia.Api.ServiceInterface/"]
COPY ["Ekklesia.Api.ServiceModel/Ekklesia.Api.ServiceModel.csproj", "Ekklesia.Api.ServiceModel/"]
RUN dotnet restore "Ekklesia.Api/Ekklesia.Api.csproj"
COPY . .
WORKDIR "/src/Ekklesia.Api"
RUN dotnet build "Ekklesia.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ekklesia.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ekklesia.Api.dll"]
