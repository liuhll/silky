FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ARG http_port=80
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${http_port}

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY NuGet.Config .
COPY sample.common.props .
COPY microservices/gateway/Silky.GatewayHost /src/microservices/gateway/Silky.GatewayHost
COPY microservices/order/Silky.Order.Application.Contracts /src/microservices/order/Silky.Order.Application.Contracts
COPY microservices/order/Silky.Order.Domain.Shared /src/microservices/order/Silky.Order.Domain.Shared
COPY microservices/account/Silky.Account.Application.Contracts /src/microservices/account/Silky.Account.Application.Contracts
COPY microservices/account/Silky.Account.Domain.Shared /src/microservices/account/Silky.Account.Domain.Shared
COPY microservices/stock/Silky.Stock.Application.Contracts /src/microservices/stock/Silky.Stock.Application.Contracts
COPY microservices/stock/Silky.Stock.Domain.Shared /src/microservices/stock/Silky.Stock.Domain.Shared

RUN dotnet restore /src/microservices/gateway/Silky.GatewayHost/Silky.GatewayHost.csproj --disable-parallel && \
    dotnet build --no-restore -c Release /src/microservices/gateway/Silky.GatewayHost/Silky.GatewayHost.csproj

FROM build AS publish
WORKDIR /src/microservices/gateway/Silky.GatewayHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Silky.GatewayHost.dll"]