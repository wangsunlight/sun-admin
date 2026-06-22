FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SunAdmin.slnx ./
COPY src/SunAdmin.Api/SunAdmin.Api.csproj src/SunAdmin.Api/
COPY src/SunAdmin.Application/SunAdmin.Application.csproj src/SunAdmin.Application/
COPY src/SunAdmin.Contracts/SunAdmin.Contracts.csproj src/SunAdmin.Contracts/
COPY src/SunAdmin.Domain/SunAdmin.Domain.csproj src/SunAdmin.Domain/
COPY src/SunAdmin.Infrastructure/SunAdmin.Infrastructure.csproj src/SunAdmin.Infrastructure/

RUN dotnet restore src/SunAdmin.Api/SunAdmin.Api.csproj

COPY src/ src/
RUN dotnet publish src/SunAdmin.Api/SunAdmin.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SunAdmin.Api.dll"]
