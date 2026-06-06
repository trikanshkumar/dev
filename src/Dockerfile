FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Packages.props ./
COPY nuget.config ./
COPY src/ ./src/

# restore + publish
RUN dotnet restore src/UPS.WWRR.APP/UPS.WWRR.App.csproj && \
    dotnet publish src/UPS.WWRR.APP/UPS.WWRR.App.csproj -c Release -o /app/publish --no-restore

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app
COPY --from=build /app/publish .

USER appuser

EXPOSE 8080

HEALTHCHECK CMD wget --no-verbose --tries=1 --spider http://localhost:8080 || exit 1

ENTRYPOINT ["dotnet", "UPS.WWRR.App.dll"]