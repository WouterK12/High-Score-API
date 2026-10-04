#!/usr/bin/env bash
# Usage: ./backup-database.sh [output-file]
set -euo pipefail
cd "$(dirname "$0")"

DATABASE_NAME="${MYSQL_DATABASE_NAME:-HighScoreAPIDev}"
mkdir -p backups
OUTPUT_FILE="${1:-backups/${DATABASE_NAME}_$(date +%Y%m%d_%H%M%S).sql}"

# Write to a temp file first so a failed dump doesn't leave a partial backup behind.
docker compose exec -T mysql sh -c 'exec mysqldump -uroot -p"$MYSQL_ROOT_PASSWORD" --single-transaction --routines --triggers --databases "$1"' sh "$DATABASE_NAME" > "$OUTPUT_FILE.tmp"
mv "$OUTPUT_FILE.tmp" "$OUTPUT_FILE"

echo "Backup written to $OUTPUT_FILE"
