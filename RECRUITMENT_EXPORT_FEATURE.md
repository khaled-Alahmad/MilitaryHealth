# ميزة تصدير البيانات لمركز التجنيد
## Recruitment Export Feature Documentation

**تاريخ الإصدار:** 17 نوفمبر 2025  
**الإصدار:** v2.1.0

---

## 📋 نظرة عامة

تم إضافة ميزة جديدة لتصدير بيانات المنتسبين لمركز التجنيد بصيغة PDF محمي (لا يمكن النسخ منه أو التعديل).

---

## ✨ الميزات الأساسية

### 1. تتبع حالة التصدير

تم إضافة حقلين جديدين في جدول `FinalDecision`:

| الحقل | النوع | الوصف |
|-------|------|-------|
| `IsExportedToRecruitment` | `BIT` (Boolean) | هل تم تصدير هذا القرار للتجنيد؟ |
| `ExportedAt` | `DATETIME` | تاريخ ووقت التصدير |

**القيمة الافتراضية:** `IsExportedToRecruitment = false`

---

### 2. طرق التصدير

#### الطريقة الأولى: تصدير محدد (Selected IDs)
```http
POST /api/RecruitmentExport/export
Content-Type: application/json

{
  "decisionIds": [1, 5, 10, 15],
  "exportAll": false
}
```

#### الطريقة الثانية: تصدير الكل (Export All)
```http
POST /api/RecruitmentExport/export-all
```
أو:
```http
POST /api/RecruitmentExport/export
Content-Type: application/json

{
  "exportAll": true
}
```

---

### 3. الحصول على القائمة المعلقة (Pending Exports)

للحصول على قائمة المنتسبين الجاهزين للتصدير (لم يتم تصديرهم بعد):

```http
GET /api/RecruitmentExport/pending
Authorization: Bearer {token}
```

**الرد:**
```json
[
  {
    "sequenceNumber": 1,
    "fileNumber": "2025-001",
    "fullName": "أحمد محمد علي",
    "motherName": "فاطمة",
    "maritalStatus": "أعزب",
    "dateOfBirth": "2000-01-15T00:00:00",
    "bloodType": "A+",
    "recruitmentCenter": "مركز دمشق",
    "result": "لائق",
    "supervisorEvaluationDate": "2025-11-17T10:30:00",
    "recommendations": null,
    "reason": "لائق طبياً"
  },
  ...
]
```

---

## 📄 محتويات ملف PDF المُصدّر

الملف يحتوي على جدول بالحقول التالية **بالترتيب**:

| # | اسم الحقل | الوصف |
|---|-----------|-------|
| 1 | التعداد | رقم تسلسلي |
| 2 | رقم الاستمارة | `FileNumber` |
| 3 | الاسم الثلاثي | `FullName` |
| 4 | اسم الأم | `MotherName` |
| 5 | الحالة الاجتماعية | `MaritalStatus` |
| 6 | المواليد | `DateOfBirth` |
| 7 | الزمرة | `BloodType` (فصيلة الدم) |
| 8 | اسم المركز | `RecruitmentCenter` |
| 9 | النتيجة | `Result` (لائق/غير لائق) |
| 10 | تاريخ التقييم | `SupervisorEvaluationDate` |
| 11 | السبب | `Reason` |

---

## 🔒 حماية ملف PDF

الملف المُنتَج يحتوي على:

- ✅ **حماية من النسخ:** لا يمكن نسخ النص من الملف
- ✅ **حماية من التعديل:** لا يمكن تعديل محتوى الملف
- ✅ **تنسيق احترافي:** تصميم جدول منظم مع header و footer
- ✅ **دعم اللغة العربية:** عرض صحيح للنص العربي
- ✅ **اسم ملف واضح:** `Recruitment_Export_20251117_143052.pdf`

---

## 🛠️ التفاصيل التقنية

### التعديلات على قاعدة البيانات

```sql
-- إضافة حقول التتبع
ALTER TABLE [dbo].[FinalDecision]
ADD [IsExportedToRecruitment] BIT NOT NULL DEFAULT 0;

ALTER TABLE [dbo].[FinalDecision]
ADD [ExportedAt] DATETIME NULL;
```

**الملف:** `src/Api/add_export_fields.sql`

---

### الملفات الجديدة

#### 1. DTOs (Application Layer)
```
src/Application/DTOs/RecruitmentExportDto.cs
```
- `RecruitmentExportDto`: بيانات التصدير
- `ExportToRecruitmentRequest`: طلب التصدير
- `ExportToRecruitmentResponse`: رد التصدير

#### 2. Interface
```
src/Application/Abstractions/IRecruitmentExportService.cs
```

#### 3. Service (Infrastructure Layer)
```
src/Infrastructure/Services/RecruitmentExportService.cs
```
- تنفيذ منطق التصدير
- إنشاء PDF باستخدام **QuestPDF**
- تطبيق حماية على الملف

#### 4. Controller (API Layer)
```
src/Api/Controllers/RecruitmentExportController.cs
```
Endpoints:
- `GET /api/RecruitmentExport/pending`
- `POST /api/RecruitmentExport/export`
- `POST /api/RecruitmentExport/export-all`

---

### المكتبات المُضافة

```xml
<PackageReference Include="QuestPDF" Version="2024.10.3" />
```

**QuestPDF** هي مكتبة مجانية ومفتوحة المصدر لإنشاء PDF باستخدام C#.

---

## 📝 خطوات التطبيق

### 1. تحديث قاعدة البيانات

نفّذ الـ SQL script:

```sql
-- الملف: src/Api/add_export_fields.sql
```

**أو** إذا كنت ستطبق كل التحديثات مرة واحدة، استخدم:

```sql
-- الملف: src/Api/migration_script_v2.sql (محدّث)
```

---

### 2. نشر التطبيق المحدّث

```bash
# Build
dotnet build MilitaryHealth.sln -c Release

# Publish
dotnet publish src/Api/Api.csproj -c Release -o publish

# Upload to server
# نقل الملفات من مجلد publish إلى السرفر
```

---

## 🧪 اختبار الميزة

### الخطوة 1: إنشاء قرارات نهائية

تأكد من وجود قرارات نهائية في النظام مع بيانات كاملة للمنتسبين.

### الخطوة 2: الحصول على القائمة المعلقة

```bash
curl -X GET "http://your-server/api/RecruitmentExport/pending" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### الخطوة 3: تصدير محدد

```bash
curl -X POST "http://your-server/api/RecruitmentExport/export" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "decisionIds": [1, 2, 3],
    "exportAll": false
  }' \
  --output recruitment_export.pdf
```

### الخطوة 4: تصدير الكل

```bash
curl -X POST "http://your-server/api/RecruitmentExport/export-all" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  --output recruitment_export_all.pdf
```

### الخطوة 5: التحقق من الحماية

- افتح الملف باستخدام PDF Reader
- حاول نسخ النص → **لن تتمكن** ✅
- حاول التعديل → **محمي** ✅

---

## 🔄 سير العمل (Workflow)

```
1. المشرف ينتهي من تقييم المنتسبين
   ↓
2. يدخل على قائمة "جاهز للتصدير"
   GET /api/RecruitmentExport/pending
   ↓
3. يختار المنتسبين المراد تصديرهم
   (أو يختار "تصدير الكل")
   ↓
4. ينقر "تصدير"
   POST /api/RecruitmentExport/export
   ↓
5. يتم إنشاء PDF محمي
   ↓
6. يتم تحديث حالة التصدير في قاعدة البيانات
   IsExportedToRecruitment = true
   ExportedAt = DateTime.Now
   ↓
7. يتم تحميل الملف
   ↓
8. يتم إرسال الملف لمركز التجنيد
```

---

## ⚠️ ملاحظات مهمة

### 1. الأمان

- ✅ يجب التحقق من صلاحيات المستخدم قبل التصدير
- ✅ يُفضل السماح فقط لـ **المشرف** بتصدير البيانات
- ✅ يتم حفظ تاريخ التصدير لأغراض التدقيق (Audit)

### 2. الأداء

- إذا كان عدد السجلات كبير جداً (أكثر من 1000)، يُفضل تصدير على دفعات
- ملف PDF يتم إنشاؤه في الذاكرة، لذا قد يستهلك موارد

### 3. التوافق

- الميزة متوافقة مع جميع البيانات الموجودة
- السجلات القديمة ستكون `IsExportedToRecruitment = false` بشكل افتراضي

---

## 🐛 استكشاف الأخطاء

### خطأ: "لا توجد قرارات للتصدير"

**السبب:** جميع القرارات تم تصديرها مسبقاً  
**الحل:** تأكد من وجود قرارات جديدة لم يتم تصديرها

### خطأ: "يجب تحديد قرارات للتصدير"

**السبب:** `decisionIds` فارغة و `exportAll = false`  
**الحل:** إما أرسل IDs أو اجعل `exportAll = true`

### خطأ: PDF فارغ أو معطوب

**السبب:** خطأ في بيانات المنتسبين  
**الحل:** تحقق من Logs وتأكد من اكتمال البيانات

---

## 📊 إحصائيات التصدير (مقترح مستقبلي)

يمكن إضافة صفحة إحصائيات تعرض:

- عدد المنتسبين المُصدّرين
- تاريخ آخر تصدير
- عدد المعلقين (لم يتم تصديرهم)
- تصدير حسب النتيجة (لائق/غير لائق)

---

## 🔗 APIs Documentation

### GET /api/RecruitmentExport/pending

**الوصف:** الحصول على قائمة المنتسبين الجاهزين للتصدير

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK`
```json
[
  {
    "sequenceNumber": 1,
    "fileNumber": "string",
    "fullName": "string",
    "motherName": "string",
    "maritalStatus": "string",
    "dateOfBirth": "2025-11-17T00:00:00",
    "bloodType": "string",
    "recruitmentCenter": "string",
    "result": "string",
    "supervisorEvaluationDate": "2025-11-17T10:00:00",
    "recommendations": "string",
    "reason": "string"
  }
]
```

---

### POST /api/RecruitmentExport/export

**الوصف:** تصدير منتسبين محددين أو الكل

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "decisionIds": [1, 2, 3],  // اختياري
  "exportAll": false
}
```

**Response:** `200 OK` - PDF File
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="Recruitment_Export_20251117_143052.pdf"
```

---

### POST /api/RecruitmentExport/export-all

**الوصف:** تصدير جميع المنتسبين غير المُصدّرين

**Headers:**
```
Authorization: Bearer {token}
```

**Response:** `200 OK` - PDF File

---

## ✅ الخلاصة

### ما تم إضافته:

1. ✅ حقلان جديدان في `FinalDecision` لتتبع التصدير
2. ✅ Service كامل للتصدير مع إنشاء PDF محمي
3. ✅ 3 Endpoints جديدة في API
4. ✅ DTOs كاملة للتصدير
5. ✅ تكامل مع QuestPDF library
6. ✅ SQL scripts للتطبيق

### الملفات المُعدّلة:

- `src/Infrastructure/Persistence/Models/FinalDecision.cs`
- `src/Application/DTOs/FinalDecisions/FinalDecisionDto.cs`
- `src/Infrastructure/Persistence/AppDbContext.cs`
- `src/Api/Program.cs`
- `src/Infrastructure/Infrastructure.csproj`

### الملفات الجديدة:

- `src/Api/add_export_fields.sql`
- `src/Application/DTOs/RecruitmentExportDto.cs`
- `src/Application/Abstractions/IRecruitmentExportService.cs`
- `src/Infrastructure/Services/RecruitmentExportService.cs`
- `src/Api/Controllers/RecruitmentExportController.cs`

---

**تم إعداد هذا التوثيق بتاريخ: 17 نوفمبر 2025**

**الإصدار:** v2.1.0  
**الميزة:** تصدير البيانات لمركز التجنيد

