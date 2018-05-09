#!/usr/bin/env bash

set -eu

cd "$(dirname "$0")"

echo "Restoring packages..."
dotnet restore build.proj

echo "Executing Fake..."
dotnet fake run build.fsx --target "$@"
