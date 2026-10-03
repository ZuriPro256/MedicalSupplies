#!/bin/zsh
set -e
PROJECT_DIR="${1:-$PWD}"
SRC="$(cd "$(dirname "$0")" && pwd)/MedicalSupplies.Web/wwwroot/images/products"
DEST="$PROJECT_DIR/MedicalSupplies.Web/wwwroot/images/products"
mkdir -p "$DEST"
cp -f "$SRC"/*.png "$DEST"/
echo "Copied $(ls -1 "$SRC"/*.png | wc -l | tr -d ' ') product images to:"
echo "$DEST"
