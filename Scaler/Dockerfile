FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 11113
EXPOSE 30002

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Scaler/Scaler.csproj", "Scaler/"]
RUN dotnet restore "Scaler/Scaler.csproj"
COPY . .
WORKDIR "/src/Scaler"
RUN dotnet build "Scaler.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Scaler.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Scaler.dll"]