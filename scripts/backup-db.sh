#!/usr/bin/env bash
# نسخ احتياطي يومي لقاعدة بيانات MilitaryHealth (SQL Server)
# يشغّل على السيرفر من مجلد المشروع، أو عبر cron الساعة 1 صباحاً
# الاستخدام: ./scripts/backup-db.sh   أو من مجلد المشروع: bash scripts/backup-db.sh

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKUP_DIR="$PROJECT_DIR/backups"
DB_NAME="db_abd789_militaryhealth"
COMPOSE_PROJECT_NAME="militaryhealth"
NETWORK="${COMPOSE_PROJECT_NAME}_default"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="db_${DB_NAME}_${TIMESTAMP}.bak"

# قراءة كلمة مرور SA من .env إن وُجد
if [ -f "$PROJECT_DIR/.env" ]; then
  set -a
  # shellcheck source=/dev/null
  source "$PROJECT_DIR/.env"
  set +a
fi

SA_PASSWORD="${MSSQL_SA_PASSWORD:-YourStrong@Passw0rd}"
export SA_PASSWORD

mkdir -p "$BACKUP_DIR"

# التأكد من أن حاوية SQL Server تعمل (أي اسم يحتوي sqlserver)
if ! docker ps --format '{{.Names}}' | grep -qi sqlserver; then
  echo "خطأ: حاوية SQL Server غير مشغّلة. شغّل: docker compose -f docker-compose.images.yml up -d"
  exit 1
fi

# تشغيل النسخ الاحتياطي عبر حاوية mssql-tools على شبكة المشروع
# المسار /backups داخل حاوية sqlserver مطابق لمجلد backups على المضيف
# sqlcmd قد يكون في /opt/mssql-tools/bin أو /opt/mssql-tools18/bin حسب إصدار الصورة
echo "جاري إنشاء النسخ الاحتياطي: $BACKUP_FILE"
docker run --rm \
  --network "$NETWORK" \
  -e SA_PASSWORD="$SA_PASSWORD" \
  -e DB_NAME="$DB_NAME" \
  -e BACKUP_FILE="$BACKUP_FILE" \
  mcr.microsoft.com/mssql-tools:latest \
  bash -c 'if [ -x /opt/mssql-tools/bin/sqlcmd ]; then SQLCMD=/opt/mssql-tools/bin/sqlcmd; elif [ -x /opt/mssql-tools18/bin/sqlcmd ]; then SQLCMD=/opt/mssql-tools18/bin/sqlcmd; else SQLCMD=sqlcmd; fi; exec "$SQLCMD" -S sqlserver -U sa -P "$SA_PASSWORD" -C -l 10 -Q "BACKUP DATABASE [$DB_NAME] TO DISK = N'\''/backups/$BACKUP_FILE'\'' WITH INIT, COMPRESSION, NOUNLOAD;"'

if [ $? -eq 0 ]; then
  echo "تم إنشاء النسخ الاحتياطي بنجاح: $BACKUP_DIR/$BACKUP_FILE"
  ls -la "$BACKUP_DIR/$BACKUP_FILE" 2>/dev/null || true
else
  echo "فشل النسخ الاحتياطي."
  exit 1
fi
