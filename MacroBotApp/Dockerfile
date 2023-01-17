FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["MacroBotApp/MacroBotApp.csproj", "MacroBotApp/"]
RUN dotnet restore "MacroBotApp/MacroBotApp.csproj"
COPY . .
WORKDIR "/src/MacroBotApp"
RUN dotnet build "MacroBotApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MacroBotApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MacroBotApp.dll"]
