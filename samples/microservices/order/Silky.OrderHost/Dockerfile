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
COPY microservices/order /src/microservices/order
COPY microservices/account/Silky.Account.Application.Contracts /src/microservices/account/Silky.Account.Application.Contracts
COPY microservices/stock/Silky.Stock.Application.Contracts /src/microservices/stock/Silky.Stock.Application.Contracts

RUN dotnet restore /src/microservices/order/Silky.OrderHost/Silky.OrderHost.csproj --disable-parallel && \
    dotnet build --no-restore -c Release /src/microservices/order/Silky.OrderHost/Silky.OrderHost.csproj

FROM build AS publish
WORKDIR /src/microservices/order/Silky.OrderHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Silky.OrderHost.dll"]