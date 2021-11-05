FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ARG rpc_port=2200
ARG ws_port=3000
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${rpc_port} ${ws_port}

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY common.props .
COPY microservices/SilkyApp /src/microservices/SilkyApp
RUN dotnet restore /src/microservices/SilkyApp/SilkyAppHost/SilkyAppHost.csproj --disable-parallel && \
    dotnet build --no-restore -c Release /src/microservices/SilkyApp/SilkyAppHost/SilkyAppHost.csproj

FROM build AS publish
WORKDIR /src/microservices/SilkyApp/SilkyAppHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SilkyAppHost.dll"]