FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GeneralHostDemo/GeneralHostDemo.csproj", "GeneralHostDemo/"]
RUN dotnet restore "GeneralHostDemo/GeneralHostDemo.csproj"
COPY . .
WORKDIR "/src/GeneralHostDemo"
RUN dotnet build "GeneralHostDemo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GeneralHostDemo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GeneralHostDemo.dll"]
