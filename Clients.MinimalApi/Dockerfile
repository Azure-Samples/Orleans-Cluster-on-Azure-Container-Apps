#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Clients.MinimalApi/Clients.MinimalApi.csproj", "Clients.MinimalApi/"]
COPY ["Abstractions/Abstractions.csproj", "Abstractions/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "Clients.MinimalApi/Clients.MinimalApi.csproj"
COPY . .
WORKDIR "/src/Clients.MinimalApi"
RUN dotnet build "Clients.MinimalApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Clients.MinimalApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clients.MinimalApi.dll"]