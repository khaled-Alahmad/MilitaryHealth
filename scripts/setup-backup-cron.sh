#!/usr/bin/env bash
# إعداد cron لنسخ احتياطي يومي الساعة 1 صباحاً
# يشغّل مرة واحدة على السيرفر: bash scripts/setup-backup-cron.sh

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKUP_SCRIPT="$SCRIPT_DIR/backup-db.sh"
CRON_LINE="0 1 * * * cd $PROJECT_DIR && bash $BACKUP_SCRIPT >> $PROJECT_DIR/backups/backup.log 2>&1"

if [ ! -f "$BACKUP_SCRIPT" ]; then
  echo "خطأ: لم يُعثر على السكربت: $BACKUP_SCRIPT"
  exit 1
fi

mkdir -p "$PROJECT_DIR/backups"
chmod +x "$BACKUP_SCRIPT"

if crontab -l 2>/dev/null | grep -q "backup-db.sh"; then
  echo "المهمة المجدولة للنسخ الاحتياطي موجودة مسبقاً في crontab."
  crontab -l | grep backup-db
  exit 0
fi

(crontab -l 2>/dev/null; echo "# MilitaryHealth DB backup daily at 1 AM"; echo "$CRON_LINE") | crontab -
echo "تمت إضافة المهمة المجدولة: نسخ احتياطي يومي الساعة 1:00 صباحاً"
echo "السطر المضاف: $CRON_LINE"
crontab -l
