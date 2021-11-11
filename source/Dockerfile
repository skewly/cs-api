FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS publish
WORKDIR /src
COPY *.csproj ./
RUN dotnet restore --source https://api.nuget.org/v3/index.json
COPY . ./
RUN dotnet publish "Skewly.WebApp.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Skewly.WebApp.dll"]