FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebSocketDemo/WebSocketDemo.csproj", "WebSocketDemo/"]
RUN dotnet restore "WebSocketDemo/WebSocketDemo.csproj"
COPY . .
WORKDIR "/src/WebSocketDemo"
RUN dotnet build "WebSocketDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebSocketDemo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebSocketDemo.dll"]
