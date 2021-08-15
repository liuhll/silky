FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ARG http_port=80
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${http_port}

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY common.props .
COPY gateways/SilkyApp.GatewayHost /src/gateways/SilkyApp.GatewayHost


RUN dotnet restore /src/gateways/SilkyApp.GatewayHost/SilkyApp.GatewayHost.csproj --disable-parallel && \
    dotnet build --no-restore -c Release /src/gateways/SilkyApp.GatewayHost/SilkyApp.GatewayHost.csproj

FROM build AS publish
WORKDIR /src/gateways/SilkyApp.GatewayHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SilkyApp.GatewayHost.dll"]