#!/usr/bin/env bash
# Instala o Unity Hub no Ubuntu/Debian e abre este projeto.
# Execute: bash install-unity.sh

set -euo pipefail

echo "=== Instalador Unity Hub (Linux) ==="

if command -v unityhub &>/dev/null; then
  echo "Unity Hub já está instalado: $(unityhub --version 2>/dev/null || echo 'ok')"
else
  echo "Adicionando repositório oficial da Unity..."
  sudo install -d /etc/apt/keyrings
  curl -fsSL https://hub.unity3d.com/linux/keys/public | sudo gpg --dearmor -o /etc/apt/keyrings/unityhub.gpg
  echo "deb [arch=amd64 signed-by=/etc/apt/keyrings/unityhub.gpg] https://hub.unity3d.com/linux/repos/deb stable main" | sudo tee /etc/apt/sources.list.d/unityhub.list
  sudo apt update
  sudo apt install -y unityhub
fi

PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo ""
echo "=== Próximos passos no Unity Hub ==="
echo "1. Abra o Unity Hub: unityhub"
echo "2. Faça login com sua conta Unity (gratuita)"
echo "3. Instale o Unity Editor 6000.0 LTS (ou superior)"
echo "4. Clique em 'Open' e selecione: $PROJECT_DIR"
echo ""
echo "Abrindo Unity Hub..."
unityhub -- --headless open "$PROJECT_DIR" 2>/dev/null || unityhub "$PROJECT_DIR" 2>/dev/null || unityhub &
