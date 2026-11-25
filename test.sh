#!/bin/sh
# Сборка в Docker, запуск на macOS. Пример: ./test.sh osx-x64
set -eu
cd "$(dirname "$0")"

case "$(uname -m)" in
    arm64) default=osx-arm64 ;;
    *) default=osx-x64 ;;
esac

rid="${1:-$default}"
case "$rid" in
    osx-arm64 | osx-x64) ;;
    *) echo "Неизвестный RID: $rid (нужен osx-arm64 или osx-x64)" >&2; exit 2 ;;
esac

rm -rf "out/$rid"
docker build --progress=plain --build-arg RID="$rid" --output "type=local,dest=out/$rid" .
"./out/$rid/IoctlMacos.Tests"
