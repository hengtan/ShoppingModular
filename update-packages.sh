#!/bin/bash

# Caminho base do script
ROOT_DIR=$(pwd)

# Lista de pacotes e versões seguras
declare -A packages=(
  ["Microsoft.Extensions.Caching.Memory"]="9.0.0"
  ["Npgsql"]="8.0.3"
  ["System.Text.Json"]="9.0.0"
  ["System.Drawing.Common"]="9.0.0"
)

echo "🔄 Atualizando pacotes em todos os .csproj encontrados..."

# Para cada .csproj do projeto
find "$ROOT_DIR" -name "*.csproj" | while read -r csproj; do
  echo "📦 Atualizando pacotes no arquivo: $csproj"
  for pkg in "${!packages[@]}"; do
    version=${packages[$pkg]}
    echo "➡️  $pkg@$version"
    dotnet add "$csproj" package "$pkg" --version "$version"
  done
done

echo "✅ Atualização concluída com sucesso."