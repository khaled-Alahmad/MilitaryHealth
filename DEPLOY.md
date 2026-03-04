# نشر المشروع بالسيرفر المحلي (Docker)

## التشغيل المحلي الكامل (API + الفرونت + SQL Server)

للبناء والتشغيل مع مشروع الفرونت (Angular) من مسار منفصل، مثلاً `P:\MilitaryHealth.UI`:

1. **من مجلد المشروع الرئيسي** `P:\MilitaryHealth`:
   ```powershell
   cd P:\MilitaryHealth
   $env:FRONTEND_CONTEXT = "P:\MilitaryHealth.UI"
   docker compose build
   docker compose down -v //reset data 

   docker compose up -d
   ```

2. **الوصول للتطبيق:**
   - الواجهة والـ API عبر البوابة: **http://localhost** (منفذ 80) — استخدم هذا للتصفح ورفع الملفات (حد 100 ميجا)
   - الـ API مباشرة: **http://localhost:8080**
   - SQL Server: `localhost:1433` (للاستخدام من أدوات خارجية)

   الطلبات إلى `http://localhost/api/...` تمر عبر خدمة **gateway** (nginx) التي تسمح بجسم طلب حتى 100 ميجا، فلا يظهر خطأ 413 عند رفع الملفات.

3. **إيقاف التشغيل:**
   ```powershell
   docker compose down
   ```

بدون تعيين `FRONTEND_CONTEXT` يُفترض أن الفرونت في المسار النسبي `../MilitaryHealth.UI` (نفس المستوى فوق مجلد المشروع).

---

## الخيار 1: رفع الصور عبر GitHub (مُفضّل)

### على جهازك (بناء الصورة ورفعها إلى GitHub Container Registry)

1. **بناء الصورة محلياً ووضع علامة لـ GHCR:**
   ```bash
   docker build -t ghcr.io/YOUR_GITHUB_USERNAME/MilitaryHealth/api:latest .
   ```

2. **تسجيل الدخول إلى GHCR** (مرة واحدة):
   ```bash
   echo $env:CR_PAT | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin
   ```
   أنشئ توكن من: GitHub → Settings → Developer settings → Personal access tokens، مع صلاحية `write:packages`.

3. **رفع الصورة:**
   ```bash
   docker push ghcr.io/YOUR_GITHUB_USERNAME/MilitaryHealth/api:latest
   ```

أو استخدم **GitHub Actions**: عند الدفع إلى `master` أو `main` تُبنى الصورة وتُرفع تلقائياً إلى `ghcr.io/YOUR_USERNAME/MilitaryHealth/api:latest`.

### على السيرفر

1. تثبيت Docker و Docker Compose.
2. إنشاء مجلد المشروع وملف `.env` (اختياري):
   ```bash
   mkdir -p ~/militaryhealth && cd ~/militaryhealth
   echo "ConnectionStrings__DefaultConnection=Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True" > .env
   echo "Jwt__Key=your-production-jwt-secret" >> .env
   ```
3. سحب الصورة وتشغيل الحاويات:
   - إذا استخدمت GHCR (صور خاصة): تسجيل الدخول أولاً:
     ```bash
     echo $CR_PAT | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin
     ```
   - إنشاء ملف `docker-compose.yml` على السيرفر (نسخ من المستودع) أو استنساخ المستودع:
     ```bash
     git clone https://github.com/YOUR_USERNAME/MilitaryHealth.git
     cd MilitaryHealth
     ```
   - تشغيل باستخدام صورة جاهزة (بدون بناء على السيرفر):
     عدّل `docker-compose.yml` واستبدل `build:` لخدمة الـ API بـ:
     ```yaml
     api:
       image: ghcr.io/YOUR_USERNAME/MilitaryHealth/api:latest
       # احذف build: ... إن وُجد
     ```
   - ثم:
     ```bash
     docker compose -f docker-compose.yml -f docker-compose.external-db.yml up -d
     ```
     (استخدم `docker-compose.external-db.yml` إذا كانت قاعدة البيانات خارج Docker.)

---

## الخيار 2: بناء على السيرفر من الكود (Git clone ثم build)

على السيرفر:

```bash
git clone https://github.com/YOUR_USERNAME/MilitaryHealth.git
cd MilitaryHealth
# ضبط .env أو متغيرات الاتصال
docker compose up -d --build
```

---

## الخيار 3: نقل الصورة يدوياً (بدون GitHub للصور)

على جهازك:

```bash
docker build -t militaryhealth-api:latest .
docker save militaryhealth-api:latest -o militaryhealth-api.tar
scp militaryhealth-api.tar user@your-server:/tmp/
```

على السيرفر:

```bash
docker load -i /tmp/militaryhealth-api.tar
cd /path/to/MilitaryHealth
# عدّل docker-compose لاستخدام الصورة: image: militaryhealth-api:latest
docker compose up -d
```

---

## إضافة مشروع الفرونت

1. أضف `Dockerfile` في مشروع الفرونت (مثلاً بناء Node ثم nginx أو خدمة ثابتة).
2. في هذا المستودع أو في مستودع واحد:
   - إما إضافة الفرونت كخدمة في `docker-compose.yml` مع `build: context: ../frontend-path`.
   - أو بناء صورة الفرونت ورفعها إلى GHCR ثم على السيرفر سحبها وتشغيلها في نفس الـ compose.

---

## النسخ الاحتياطي اليومي

لإعداد نسخ احتياطي يومي تلقائي لقاعدة البيانات **الساعة 1 صباحاً** وحفظه في مجلد على السيرفر، راجع **[BACKUP.md](BACKUP.md)**. يشمل الإعداد سكربتات النسخ، إعداد cron، و**اختبار أولي** للتأكد من أن النسخ يعمل على السيرفر.

---

## ملاحظات

- **قاعدة البيانات:** إذا كانت SQL على السيرفر (خارج Docker)، استخدم:
  `docker compose -f docker-compose.yml -f docker-compose.external-db.yml up -d`
  وضبط `ConnectionStrings__DefaultConnection` في `.env`.
- **الهجرة (Migrations):** نفّذها يدوياً على السيرفر أو أضف خطوة في الـ API تطلق الهجرات عند التشغيل إذا رغبت.
- **الفرونت:** يمكن استضافة الملفات الثابتة من الـ API أو من حاوية nginx منفصلة تشير إلى الـ API.
