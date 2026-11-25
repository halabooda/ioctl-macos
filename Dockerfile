# Linux-контейнер только собирает. Запускать результат нужно на macOS.
FROM mcr.microsoft.com/dotnet/sdk:10.0.404 AS build
ARG RID=osx-arm64
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
WORKDIR /repo
COPY Directory.Build.props .editorconfig ./
COPY src/IoctlMacos/IoctlMacos.csproj src/IoctlMacos/
COPY tests/IoctlMacos.Tests/IoctlMacos.Tests.csproj tests/IoctlMacos.Tests/
RUN dotnet restore tests/IoctlMacos.Tests -r $RID
COPY src src
COPY tests tests
RUN dotnet publish tests/IoctlMacos.Tests -c Release -r $RID --self-contained --no-restore -o /out

FROM scratch AS export
COPY --from=build /out /
