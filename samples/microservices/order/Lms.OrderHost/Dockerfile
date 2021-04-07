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
COPY microservices/order /src/microservices/order
COPY microservices/account/Lms.Account.Application.Contracts /src/microservices/account/Lms.Account.Application.Contracts
COPY microservices/stock/Lms.Stock.Application.Contracts /src/microservices/stock/Lms.Stock.Application.Contracts

RUN dotnet restore /src/microservices/order/Lms.OrderHost/Lms.OrderHost.csproj && \
    dotnet build --no-restore -c Release /src/microservices/order/Lms.OrderHost/Lms.OrderHost.csproj

FROM build AS publish
WORKDIR /src/microservices/order/Lms.OrderHost
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Lms.OrderHost.dll"]