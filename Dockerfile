FROM mcr.microsoft.com/dotnet/sdk:8.0.101-bookworm-slim-amd64

WORKDIR /src

COPY ./Macro-Bot.sln .
COPY ./Directory.Build.props .
COPY ./Directory.Packages.props .
COPY ./src/MacroBot/MacroBot.csproj src/MacroBot/
COPY ./src/MacroBot.Core/MacroBot.Core.csproj src/MacroBot.Core/

COPY ./tests/MacroBot.Tests.UnitTests/MacroBot.Tests.UnitTests.csproj tests/MacroBot.Tests.UnitTests/

RUN dotnet restore
COPY . /src