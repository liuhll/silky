FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
ARG http_port=80
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${http_port}

FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build
WORKDIR /src
COPY sample.common.props .
COPY aggregators/Lms.GatewayHost /src/aggregators/Lms.GatewayHost
COPY microservices/order/Lms.Order.Application.Contracts /src/microservices/order/Lms.Order.Application.Contracts
COPY microservices/order/Lms.Order.Domain.Shared /src/microservices/order/Lms.Order.Domain.Shared
COPY microservices/account/Lms.Account.Application.Contracts /src/microservices/account/Lms.Account.Application.Contracts
COPY microservices/account/Lms.Account.Domain.Shared /src/microservices/account/Lms.Account.Domain.Shared
COPY microservices/stock/Lms.Stock.Application.Contracts /src/microservices/stock/Lms.Stock.Application.Contracts
COPY microservices/stock/Lms.Stock.Domain.Shared /src/microservices/stock/Lms.Stock.Domain.Shared

RUN dotnet restore /src/aggregators/Lms.GatewayHost/Lms.GatewayHost.csproj && \
    dotnet build --no-restore -c Release /src/aggregators/Lms.GatewayHost/Lms.GatewayHost.csproj

FROM build AS publish
WORKDIR /src/aggregators/Lms.GatewayHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Lms.GatewayHost.dll"]