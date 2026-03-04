# النسخ الاحتياطي اليومي لقاعدة البيانات

يتم نسخ قاعدة بيانات SQL Server احتياطياً تلقائياً **كل يوم الساعة 1:00 صباحاً** وحفظ الملفات في مجلد `backups/` على السيرفر.

## إذا لم تكن السكربتات موجودة على السيرفر

السكربتات (`backup-db.sh`, `test-backup.sh`, `setup-backup-cron.sh`) يجب أن تكون داخل `~/MilitaryHealth/scripts/`.

- **إذا كنت تنشر بـ `deploy-to-server.ps1`:** السكربتات تُرفع تلقائياً مع كل نشر، لا حاجة لـ Git على السيرفر. بعد النشر شغّل على السيرفر: `bash scripts/test-backup.sh`
- إذا نَسخت المشروع يدوياً بدون النشر السكربتي، انسخ الملفات عبر SCP من جهازك:
  ```powershell
  scp scripts/backup-db.sh scripts/test-backup.sh scripts/setup-backup-cron.sh omar@192.168.1.102:~/MilitaryHealth/scripts/
  ```
  ثم على السيرفر: `bash scripts/test-backup.sh`

- **إذا ظهرت أخطاء مثل `$'\r': command not found`:** الملفات منسوخة بنهايات أسطر Windows. نفّذ على السيرفر مرة واحدة ثم جرّب الاختبار:
  ```bash
  sed -i 's/\r$//' ~/MilitaryHealth/scripts/*.sh
  bash scripts/test-backup.sh
  ```
  (سكربت النشر `deploy-to-server.ps1` يفعل هذا التحويل تلقائياً بعد كل رفع.)

- **إذا ظهر "Access is denied" أو "Operating system error 5" عند النسخ الاحتياطي:** صلاحيات مجلد `backups` غير مناسبة. من مجلد المشروع نفّذ (بترتيب: chmod ثم chown):
  ```bash
  cd ~/MilitaryHealth
  chmod 700 backups
  sudo chown 10001:0 backups
  ```
  ثم أعد تشغيل: `bash scripts/test-backup.sh`

## المتطلبات على السيرفر

- تشغيل المشروع بـ Docker Compose (مثلاً `docker compose -f docker-compose.images.yml up -d`).
- أن يكون مجلد المشروع هو نفس المجلد الذي تُنفَّذ فيه أوامر الـ compose (مثلاً `~/MilitaryHealth`).

## الإعداد (مرة واحدة)

1. **رفع السكربتات إلى السيرفر**  
   إذا كنت تستخدم `git clone` أو نشر من المستودع، فمجلد `scripts/` موجود. أو انسخ الملفات:
   - `scripts/backup-db.sh`
   - `scripts/setup-backup-cron.sh`
   - `scripts/test-backup.sh`

2. **تشغيل الحاويات مع مجلد النسخ الاحتياطي**  
   ملفات `docker-compose.yml` و `docker-compose.images.yml` تحتوي على حجم (volume) لـ `./backups`. أعد تشغيل الـ compose إن لزم:
   ```bash
   cd ~/MilitaryHealth   # أو مسار مشروعك
   docker compose -f docker-compose.images.yml up -d
   ```

3. **صلاحيات مجلد النسخ الاحتياطي (مرة واحدة)**  
   حاوية SQL Server تكتب الملفات بالمستخدم `mssql` (UID 10001). من **مجلد المشروع** (وليس من داخل `backups`):
   ```bash
   cd ~/MilitaryHealth
   mkdir -p backups
   chmod 700 backups
   sudo chown 10001:0 backups
   ```
   (يجب تنفيذ `chmod` قبل `chown` لأن بعد `chown` المجلد يصبح مملوكاً لـ 10001 ولا يستطيع omar تنفيذ chmod.)

4. **إعداد Cron للنسخ الاحتياطي الساعة 1 صباحاً**
   ```bash
   cd ~/MilitaryHealth
   bash scripts/setup-backup-cron.sh
   ```
   هذا يضيف سطراً في `crontab` يشغّل `backup-db.sh` يومياً الساعة 1:00.

5. **(اختياري) كلمة مرور SA في .env**  
   إذا كانت كلمة مرور SQL Server مختلفة عن الافتراضية، ضعها في ملف `.env` في مجلد المشروع:
   ```bash
   echo "MSSQL_SA_PASSWORD=YourActualPassword" >> .env
   ```

## اختبار أولي على السيرفر

قبل الاعتماد على الجدولة، تأكد أن النسخ الاحتياطي يعمل:

```bash
cd ~/MilitaryHealth
bash scripts/test-backup.sh
```

السكربت يفعل التالي:

1. يتحقق من أن حاوية SQL Server تعمل.
2. يشغّل نسخة احتياطية واحدة.
3. يتحقق من ظهور ملف `.bak` في `backups/` ويطبع اسمه وحجمه.

إذا انتهى بدون أخطاء، فالنسخ الاحتياطي يعمل ويمكن الاعتماد على المهمة المجدولة.

## التشغيل اليدوي

لتشغيل نسخة احتياطية يدوياً في أي وقت:

```bash
cd ~/MilitaryHealth
bash scripts/backup-db.sh
```

الملفات تُحفظ في `backups/` بأسماء مثل:  
`db_db_abd789_militaryhealth_YYYYMMDD_HHMMSS.bak`

لعرض محتويات المجلد (مملوك لـ SQL Server) استخدم: `sudo ls -la ~/MilitaryHealth/backups`

## مكان الملفات والجدولة

| العنصر | الوصف |
|--------|--------|
| مجلد النسخ | `~/MilitaryHealth/backups/` (أو مسار المشروع + `backups/`) |
| وقت الجدولة | 1:00 صباحاً كل يوم (توقيت السيرفر) |
| سجل التشغيل | `backups/backup.log` (إن استخدمت `setup-backup-cron.sh` كما هو) |

## استعادة من نسخة احتياطية

على السيرفر، من داخل شبكة Docker أو من مضيف لديه وصول إلى SQL Server:

```bash
# نسخ الملف إلى مسار داخل الحاوية إن لزم، ثم استعادة عبر sqlcmd أو من داخل الحاوية
docker exec -it <sqlserver-container-name> /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'YourPassword' -C -Q "
RESTORE DATABASE [db_abd789_militaryhealth] FROM DISK = N'/backups/db_db_abd789_militaryhealth_YYYYMMDD_HHMMSS.bak'
WITH REPLACE, RECOVERY;"
```

استبدل اسم الحاوية واسم الملف والتاريخ المناسب.
