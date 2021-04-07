FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ARG rpc_port=2200
ARG ws_port=3000
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${rpc_port} ${ws_port}


FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build
WORKDIR /src
COPY sample.common.props .
COPY microservices/stock /src/microservices/stock
RUN dotnet restore /src/microservices/stock/Lms.StockHost/Lms.StockHost.csproj && \
    dotnet build --no-restore -c Release /src/microservices/stock/Lms.StockHost/Lms.StockHost.csproj

FROM build AS publish
WORKDIR /src/microservices/stock/Lms.StockHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Lms.StockHost.dll"]