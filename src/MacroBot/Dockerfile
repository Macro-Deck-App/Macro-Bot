ARG BASE_IMAGE
FROM mcr.microsoft.com/dotnet/aspnet:8.0.1-bookworm-slim-amd64 AS base
WORKDIR /app

FROM ${BASE_IMAGE} AS build
WORKDIR /src/src/MacroBot/

RUN dotnet msbuild "MacroBot.csproj" -warnAsMessage:CS0618 -p:Configuration=Release

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

RUN rm -f Config/config.json
RUN mkdir -p Config

ENTRYPOINT ["dotnet", "MacroBot.dll" ]