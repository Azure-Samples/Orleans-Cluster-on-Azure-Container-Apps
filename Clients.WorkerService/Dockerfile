#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Clients.WorkerService/Clients.WorkerService.csproj", "Clients.WorkerService/"]
COPY ["Abstractions/Abstractions.csproj", "Abstractions/"]
COPY ["Grains/Grains.csproj", "Grains/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "Clients.WorkerService/Clients.WorkerService.csproj"
COPY . .
WORKDIR "/src/Clients.WorkerService"
RUN dotnet build "Clients.WorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Clients.WorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clients.WorkerService.dll"]