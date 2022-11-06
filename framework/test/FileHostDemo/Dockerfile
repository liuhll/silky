FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
ARG rpc_port=2200
ENV TZ=Asia/Shanghai 
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone 
EXPOSE ${rpc_port}

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /
COPY framework/src framework/src
COPY framework/test/GatewayDemo framework/test/GatewayDemo
COPY framework/test/IAnotherApplication framework/test/IAnotherApplication
COPY framework/test/ITestApplication framework/test/ITestApplication
COPY NuGet.Config NuGet.Config
COPY common.props common.props
COPY Directory.Build.props Directory.Build.props
RUN dotnet restore /framework/test/GatewayDemo/GatewayDemo.csproj --disable-parallel && \
    dotnet build --no-restore -c Release /framework/test/GatewayDemo/GatewayDemo.csproj 

FROM build AS publish
WORKDIR /framework/test/GatewayDemo
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "FileHostDemo.dll"]
   