FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY SecurityEventManagement.sln ./
COPY src/SecurityEventManagement.Api/SecurityEventManagement.Api.csproj src/SecurityEventManagement.Api/
COPY tests/SecurityEventManagement.Api.Tests/SecurityEventManagement.Api.Tests.csproj tests/SecurityEventManagement.Api.Tests/
RUN dotnet restore SecurityEventManagement.sln
COPY . .
RUN dotnet publish src/SecurityEventManagement.Api/SecurityEventManagement.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
COPY --from=build /app/publish .
USER appuser
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "SecurityEventManagement.Api.dll"]
