FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy only required source
COPY src/ ./src/

ARG JFROG_NUGET_URL
ARG JFROG_USERNAME
ARG JFROG_ACCESS_TOKEN

# restore + publish
RUN dotnet nuget add source $JFROG_NUGET_URL --name jfrog --username $JFROG_USERNAME --password $JFROG_ACCESS_TOKEN --store-password-in-clear-text && \
    dotnet restore src/UPS.WWRR.APP/UPS.WWRR.App.csproj --source $JFROG_NUGET_URL && \
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