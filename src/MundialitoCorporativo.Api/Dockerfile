# Forzar Linux (evita contenedores Windows en Docker Desktop)
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MundialitoCorporativo.Api/MundialitoCorporativo.Api.csproj", "MundialitoCorporativo.Api/"]
COPY ["src/MundialitoCorporativo.Application/MundialitoCorporativo.Application.csproj", "MundialitoCorporativo.Application/"]
COPY ["src/MundialitoCorporativo.Domain/MundialitoCorporativo.Domain.csproj", "MundialitoCorporativo.Domain/"]
COPY ["src/MundialitoCorporativo.Infrastructure/MundialitoCorporativo.Infrastructure.csproj", "MundialitoCorporativo.Infrastructure/"]
RUN dotnet restore "MundialitoCorporativo.Api/MundialitoCorporativo.Api.csproj"
COPY src/ .
WORKDIR "/src/MundialitoCorporativo.Api"
RUN dotnet build "MundialitoCorporativo.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MundialitoCorporativo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MundialitoCorporativo.Api.dll"]
