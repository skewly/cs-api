FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Skewly.WebApp.csproj", ""]
RUN dotnet restore "Skewly.WebApp.csproj" --source https://api.nuget.org/v3/index.json
COPY . .
WORKDIR "/src/"
RUN dotnet build "Skewly.WebApp.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Skewly.WebApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Skewly.WebApp.dll"]