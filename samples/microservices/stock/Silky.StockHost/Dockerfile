FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ARG rpc_port=2200
ARG ws_port=3000
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${rpc_port} ${ws_port}


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY NuGet.Config .
COPY sample.common.props .
COPY microservices/stock /src/microservices/stock
RUN dotnet restore /src/microservices/stock/Silky.StockHost/Silky.StockHost.csproj --disable-parallel && \
    dotnet build --no-restore -c Release /src/microservices/stock/Silky.StockHost/Silky.StockHost.csproj

FROM build AS publish
WORKDIR /src/microservices/stock/Silky.StockHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Silky.StockHost.dll"]