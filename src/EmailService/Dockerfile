FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 9091

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/EmailService/EmailService.csproj", "src/EmailService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/EmailService/EmailService.csproj"
COPY . .
WORKDIR "/src/src/EmailService"
RUN dotnet build "EmailService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailService.dll"]

