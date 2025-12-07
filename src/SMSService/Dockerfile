FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 9092

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SMSService/SMSService.csproj", "src/SMSService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/SMSService/SMSService.csproj"
COPY . .
WORKDIR "/src/src/SMSService"
RUN dotnet build "SMSService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SMSService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SMSService.dll"]

