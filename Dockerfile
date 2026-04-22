FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY src/ ./src/

RUN dotnet restore && \
    dotnet publish -c Release -o /app/publish --runtime linux-x64 --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

RUN addgroup -S appgroup && adduser -S appuser -G appgroup

WORKDIR /app
COPY --from=build /app/publish .

USER appuser

EXPOSE 8080

HEALTHCHECK CMD wget --no-verbose --tries=1 --spider http://localhost:8080 || exit 1

ENTRYPOINT ["dotnet", "UPS.WWRR.App.dll"]