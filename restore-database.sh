#!/usr/bin/env bash
# Usage: ./restore-database.sh <backup-file>
set -euo pipefail
cd "$(dirname "$0")"

if [ $# -ne 1 ] || [ ! -f "$1" ]; then
    echo "Usage: $0 <backup-file>" >&2
    exit 1
fi

read -r -p "This will overwrite the database with '$1'. Continue? [y/N] " CONFIRM
[[ "$CONFIRM" =~ ^[yY]$ ]] || { echo "Aborted."; exit 1; }

docker compose exec -T mysql sh -c 'exec mysql -uroot -p"$MYSQL_ROOT_PASSWORD"' < "$1"

echo "Database restored from $1"
