FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/TenderAnalytics.Api/TenderAnalytics.Api.csproj", "src/TenderAnalytics.Api/"]
COPY ["src/TenderAnalytics.Application/TenderAnalytics.Application.csproj", "src/TenderAnalytics.Application/"]
COPY ["src/TenderAnalytics.Domain/TenderAnalytics.Domain.csproj", "src/TenderAnalytics.Domain/"]
COPY ["src/TenderAnalytics.Infrastructure/TenderAnalytics.Infrastructure.csproj", "src/TenderAnalytics.Infrastructure/"]

RUN dotnet restore "src/TenderAnalytics.Api/TenderAnalytics.Api.csproj"

COPY . .

RUN dotnet publish \
    "src/TenderAnalytics.Api/TenderAnalytics.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TenderAnalytics.Api.dll"]