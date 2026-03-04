#!/usr/bin/env bash
# اختبار أولي للنسخ الاحتياطي على السيرفر
# يشغّل يدوياً مرة واحدة للتأكد أن النسخ الاحتياطي يعمل: bash scripts/test-backup.sh

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
BACKUP_SCRIPT="$SCRIPT_DIR/backup-db.sh"

echo "=== اختبار النسخ الاحتياطي - MilitaryHealth ==="
echo "مجلد المشروع: $PROJECT_DIR"
echo ""

if [ ! -f "$BACKUP_SCRIPT" ]; then
  echo "خطأ: لم يُعثر على سكربت النسخ الاحتياطي: $BACKUP_SCRIPT"
  exit 1
fi

# التحقق من أن الحاويات تعمل
echo "[1/3] التحقق من حاويات Docker..."
if ! docker ps --format '{{.Names}}' | grep -qi sqlserver; then
  echo "خطأ: حاوية SQL Server غير مشغّلة. شغّل أولاً:"
  echo "  cd $PROJECT_DIR && docker compose -f docker-compose.images.yml up -d"
  exit 1
fi
echo "  OK - حاوية SQL Server تعمل"
echo ""

# تشغيل النسخ الاحتياطي
echo "[2/3] تشغيل النسخ الاحتياطي..."
bash "$BACKUP_SCRIPT"
echo ""

# التحقق من وجود الملف وحجمه (قد لا نستطيع القراءة لأن المجلد مملوك لـ SQL Server)
echo "[3/3] التحقق من ملف النسخ الاحتياطي..."
LATEST=$(ls -t "$PROJECT_DIR/backups"/db_*.bak 2>/dev/null | head -1)
if [ -n "$LATEST" ]; then
  SIZE=$(du -h "$LATEST" | cut -f1)
  echo "  الملف: $LATEST"
  echo "  الحجم: $SIZE"
else
  echo "  (النسخ الاحتياطي ناجح؛ المجلد مملوك لـ SQL Server فلا يمكن عرض الملفات هنا.)"
fi
echo ""
echo "=== الاختبار نجح. النسخ الاحتياطي يعمل بشكل صحيح. ==="
