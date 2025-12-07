FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 9090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/GatewayService/GatewayService.csproj", "src/GatewayService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/GatewayService/GatewayService.csproj"
COPY . .
WORKDIR "/src/src/GatewayService"
RUN dotnet build "GatewayService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GatewayService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GatewayService.dll"]

