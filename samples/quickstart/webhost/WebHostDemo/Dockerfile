FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WebHostDemo/WebHostDemo.csproj", "WebHostDemo/"]
RUN dotnet restore "WebHostDemo/WebHostDemo.csproj"
COPY . .
WORKDIR "/src/WebHostDemo"
RUN dotnet build "WebHostDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebHostDemo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebHostDemo.dll"]
