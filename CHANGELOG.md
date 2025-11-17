# سجل التعديلات - نظام الصحة العسكرية
## Military Health System - Changelog

**تاريخ الإصدار:** 17 نوفمبر 2025  
**رقم الإصدار:** v2.0.0

---

## 📋 جدول المحتويات

- [التعديلات على قاعدة البيانات](#التعديلات-على-قاعدة-البيانات)
- [التعديلات على الكود](#التعديلات-على-الكود)
- [التحسينات التقنية](#التحسينات-التقنية)
- [الملفات المعدّلة](#الملفات-المعدلة)
- [خطوات التطبيق](#خطوات-التطبيق)

---

## 🗄️ التعديلات على قاعدة البيانات

### 1. جدول المنتسبين (Applicants)

#### ✅ حقول جديدة:

| اسم الحقل | النوع | الوصف | إلزامي |
|-----------|------|-------|--------|
| `MotherName` | `VARCHAR(100)` | اسم الأم | ❌ اختياري |
| `DateOfBirth` | `DATETIME` | تاريخ المواليد | ❌ اختياري |
| `RecruitmentCenter` | `VARCHAR(200)` | مركز التجنيد | ❌ اختياري |
| `BloodType` | `VARCHAR(10)` | زمرة الدم (فصيلة الدم) | ❌ اختياري |
| `QueueNumber` | `INT` | رقم الدور (يتولد تلقائياً) | ❌ اختياري |

#### 🔄 آلية رقم الدور (Queue Number):
- **يتم توليده تلقائياً** عند إضافة منتسب جديد
- **يبدأ من 1 كل يوم** ويرجع للـ 1 في اليوم التالي
- يستخدم Trigger في قاعدة البيانات: `trg_GenerateQueueNumber`

**مثال:**
```
اليوم:  المنتسب 1 → رقم دور = 1
        المنتسب 2 → رقم دور = 2
        المنتسب 3 → رقم دور = 3
        
غداً:  المنتسب 1 → رقم دور = 1 (يعيد البدء)
```

---

### 2. جدول الفحص الباطني (InternalExam)

#### ❌ حقول محذوفة:

| اسم الحقل | السبب |
|-----------|-------|
| `Hearing` (السمع) | تم نقله لقسم آخر |

---

### 3. جدول القرار النهائي (FinalDecision)

#### ✅ حقول جديدة:

| اسم الحقل | النوع | الوصف |
|-----------|------|-------|
| `ReceptionAddedAt` | `DATETIME` | تاريخ الإضافة من الريسبشن |
| `SupervisorAddedAt` | `DATETIME` | تاريخ الإضافة من المشرف |
| `SupervisorLastModifiedAt` | `DATETIME` | تاريخ آخر تعديل من المشرف |

**الغرض:** تتبع دقيق لمراحل معالجة المنتسب من الريسبشن حتى المشرف.

---

### 4. جدول الاستشارات (Consultations)

#### ❌ حقول محذوفة:
- `ReferredDoctor` (الطبيب المشار إليه)

#### ✅ حقول جديدة:

| اسم الحقل | النوع | الوصف |
|-----------|------|-------|
| `ReferralReason` | `TEXT` | السبب المحال إليه (اختياري) |

**التغيير:** استبدال حقل الطبيب بحقل نصي حر لكتابة السبب.

---

### 5. جدول التحاليل (Investigations)

#### ✅ حقول جديدة:

| اسم الحقل | النوع | الوصف |
|-----------|------|-------|
| `InvestigationReason` | `TEXT` | سبب التحليل (اختياري) |

---

## 💻 التعديلات على الكود

### 1. Models (النماذج)

#### تم تحديث الملفات التالية:
- `src/Infrastructure/Persistence/Models/Applicant.cs`
- `src/Infrastructure/Persistence/Models/InternalExam.cs`
- `src/Infrastructure/Persistence/Models/FinalDecision.cs`
- `src/Infrastructure/Persistence/Models/Consultation.cs`
- `src/Infrastructure/Persistence/Models/Investigation.cs`

---

### 2. DTOs (كائنات نقل البيانات)

#### تم تحديث الملفات التالية:

**للمنتسبين:**
- `src/Application/DTOs/Applicants/ApplicantDto.cs`
- `src/Application/DTOs/Applicants/ApplicantRequest.cs`
- `src/Application/DTOs/Applicants/ApplicantDetailsDto.cs`

**للفحص الباطني:**
- `src/Application/DTOs/InternalExams/InternalExamDto.cs`
- `src/Application/DTOs/InternalExams/InternalExamRequest.cs`

**للقرار النهائي:**
- `src/Application/DTOs/FinalDecisions/FinalDecisionDto.cs`
- `src/Application/DTOs/FinalDecisions/FinalDecisionRequest.cs`

**للاستشارات:**
- `src/Application/DTOs/Consultations/ConsultationDto.cs`
- `src/Application/DTOs/Consultations/ConsultationRequest.cs`

**للتحاليل:**
- `src/Application/DTOs/Investigations/InvestigationDto.cs`
- `src/Application/DTOs/Investigations/InvestigationRequest.cs`

---

### 3. Services (الخدمات)

#### `src/Infrastructure/Services/ApplicantService.cs`
- تحديث Mapping للحقول الجديدة
- إزالة mapping لحقل `Hearing`
- تحديث mapping للاستشارات والتحاليل

---

### 4. Database Context

#### `src/Infrastructure/Persistence/AppDbContext.cs`

**تحديثات هامة:**

```csharp
// تكوين جدول المنتسبين
entity.Property(e => e.MotherName).HasMaxLength(100).IsUnicode(false);
entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
entity.Property(e => e.RecruitmentCenter).HasMaxLength(200).IsUnicode(false);
entity.Property(e => e.BloodType).HasMaxLength(10).IsUnicode(false);
entity.Property(e => e.QueueNumber);

// ✅ تكوين مهم: إخبار EF عن وجود Trigger
entity.ToTable(tb => tb.HasTrigger("trg_GenerateQueueNumber"));
```

**السبب:** عند وجود Trigger، يجب إخبار Entity Framework لتجنب استخدام `OUTPUT` clause.

---

## 🚀 التحسينات التقنية

### 1. اتصال قاعدة البيانات (Database Connection)

#### `src/Api/Program.cs`

**إضافة Retry Logic:**

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));
```

**الفائدة:**
- ✅ إعادة المحاولة تلقائياً عند فشل الاتصال
- ✅ تحمل الأخطاء المؤقتة (Transient Errors)
- ✅ استقرار أفضل للتطبيق

---

### 2. Database Triggers

#### Trigger: `trg_GenerateQueueNumber`

**الوظيفة:** توليد رقم الدور تلقائياً لكل منتسب جديد

**الميزات:**
- يعمل تلقائياً عند إدراج سجل جديد
- يحسب الرقم بناءً على **اليوم الحالي فقط**
- يبدأ من 1 كل يوم

**الكود:**
```sql
CREATE TRIGGER [dbo].[trg_GenerateQueueNumber]
ON [dbo].[Applicants]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @MaxQueueNumber INT;
    DECLARE @Today DATE = CAST(GETDATE() AS DATE);
    
    -- البحث عن أعلى رقم دور اليوم فقط
    SELECT @MaxQueueNumber = ISNULL(MAX(QueueNumber), 0)
    FROM [dbo].[Applicants]
    WHERE CAST(CreatedAt AS DATE) = @Today
      AND ApplicantID NOT IN (SELECT ApplicantID FROM inserted);
    
    -- تعيين الرقم الجديد
    ;WITH NumberedInserted AS (
        SELECT 
            ApplicantID,
            ROW_NUMBER() OVER (ORDER BY ApplicantID) AS RowNum
        FROM inserted
    )
    UPDATE a
    SET a.QueueNumber = @MaxQueueNumber + n.RowNum
    FROM [dbo].[Applicants] a
    INNER JOIN NumberedInserted n ON a.ApplicantID = n.ApplicantID
    WHERE a.QueueNumber IS NULL;
END
```

---

## 📁 الملفات المعدّلة

### قاعدة البيانات:
```
src/Api/
├── migration_script_v2.sql                    (✅ جديد - Migration شامل)
└── update_queue_trigger_daily_reset.sql       (✅ جديد - تحديث Trigger)
```

### Models:
```
src/Infrastructure/Persistence/Models/
├── Applicant.cs                               (🔄 معدّل)
├── InternalExam.cs                            (🔄 معدّل)
├── FinalDecision.cs                           (🔄 معدّل)
├── Consultation.cs                            (🔄 معدّل)
└── Investigation.cs                           (🔄 معدّل)
```

### DTOs:
```
src/Application/DTOs/
├── Applicants/
│   ├── ApplicantDto.cs                        (🔄 معدّل)
│   ├── ApplicantRequest.cs                    (🔄 معدّل)
│   └── ApplicantDetailsDto.cs                 (🔄 معدّل)
├── InternalExams/
│   ├── InternalExamDto.cs                     (🔄 معدّل)
│   └── InternalExamRequest.cs                 (🔄 معدّل)
├── FinalDecisions/
│   ├── FinalDecisionDto.cs                    (🔄 معدّل)
│   └── FinalDecisionRequest.cs                (🔄 معدّل)
├── Consultations/
│   ├── ConsultationDto.cs                     (🔄 معدّل)
│   └── ConsultationRequest.cs                 (🔄 معدّل)
└── Investigations/
    ├── InvestigationDto.cs                    (🔄 معدّل)
    └── InvestigationRequest.cs                (🔄 معدّل)
```

### Infrastructure:
```
src/Infrastructure/
├── Persistence/
│   └── AppDbContext.cs                        (🔄 معدّل - إضافة Trigger config)
└── Services/
    └── ApplicantService.cs                    (🔄 معدّل)
```

### Configuration:
```
src/Api/
└── Program.cs                                 (🔄 معدّل - إضافة Retry Logic)
```

---

## 📝 خطوات التطبيق

### الخطوة 1: تطبيق تغييرات قاعدة البيانات

#### الطريقة الأولى (إذا لم تطبق أي تعديلات سابقاً):
```sql
-- نفّذ هذا الملف على قاعدة البيانات
src/Api/migration_script_v2.sql
```

#### الطريقة الثانية (إذا طبّقت Migration سابقاً بدون Daily Reset):
```sql
-- نفّذ هذا الملف فقط لتحديث الـ Trigger
src/Api/update_queue_trigger_daily_reset.sql
```

---

### الخطوة 2: نشر التطبيق المحدّث

```bash
# 1. Build المشروع
dotnet build MilitaryHealth.sln -c Release

# 2. Publish المشروع
dotnet publish src/Api/Api.csproj -c Release -o publish

# 3. نقل الملفات من مجلد publish إلى السرفر
```

**موقع الملفات المنشورة:**
```
P:\MilitaryHealth\src\Api\bin\Release\net9.0\publish\
```

---

### الخطوة 3: التحقق من التطبيق

#### اختبار 1: إضافة منتسب جديد
```http
POST /api/Applicants
{
  "fullName": "أحمد محمد",
  "motherName": "فاطمة",
  "dateOfBirth": "2000-01-01",
  "recruitmentCenter": "مركز دمشق",
  "bloodType": "A+",
  ...
}
```

**النتيجة المتوقعة:**
```json
{
  "queueNumber": 1,  // ✅ تم التوليد تلقائياً
  "motherName": "فاطمة",
  "dateOfBirth": "2000-01-01",
  ...
}
```

#### اختبار 2: التحقق من Reset اليومي
```sql
-- اليوم
SELECT QueueNumber, FullName, CreatedAt 
FROM Applicants 
WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY QueueNumber;

-- النتيجة: 1, 2, 3, 4, ...

-- غداً (بعد إضافة منتسب جديد)
-- النتيجة: 1, 2, 3, ... (يبدأ من جديد)
```

---

## ⚠️ ملاحظات مهمة

### 1. التوافق مع الإصدارات السابقة
- ✅ جميع الحقول الجديدة **اختيارية** (Nullable)
- ✅ لن تتأثر البيانات الموجودة
- ✅ السجلات القديمة ستعمل بدون مشاكل

### 2. رقم الدور للسجلات القديمة
- السجلات التي تم إضافتها **قبل** تطبيق الـ Migration ستحتفظ بأرقام الدور الأصلية
- الـ Daily Reset سيطبّق فقط على السجلات **الجديدة**

### 3. الملف المرسل للتجنيد
حسب المتطلبات، يجب أن يحتوي الملف على:
1. اسم الأم (MotherName)
2. تاريخ المواليد (DateOfBirth)
3. زمرة الدم (BloodType)
4. مركز التجنيد (RecruitmentCenter)

**ملاحظة:** هذه الميزة تحتاج لتطبيق في الكود (لم يتم بعد).

### 4. الطباعة في الديوان
**متطلب معلق:** عرض التحاليل والنتائج والأسباب في التقرير المطبوع.

---

## 🔄 التعديلات المستقبلية المقترحة

### 1. ملف التجنيد
- [ ] إنشاء endpoint لتصدير بيانات المنتسب بالحقول المطلوبة
- [ ] تنسيق الملف حسب متطلبات مركز التجنيد

### 2. تقرير الطباعة المحسّن
- [ ] تحديث واجهة الطباعة لعرض جميع الحقول الجديدة
- [ ] إضافة التحاليل والأسباب في التقرير المطبوع

### 3. تحسينات UI
- [ ] إضافة حقول الإدخال الجديدة في واجهة الريسبشن
- [ ] عرض رقم الدور في لوحة الانتظار
- [ ] إضافة تنبيهات عند Reset رقم الدور يومياً

---

## 📞 الدعم الفني

**في حالة وجود مشاكل:**

1. **خطأ عند إضافة منتسب:**
   - تحقق من تطبيق الـ Trigger على قاعدة البيانات
   - تحقق من تكوين `AppDbContext` للـ Trigger

2. **رقم الدور لا يتولد:**
   - تحقق من وجود `CreatedAt` في السجل
   - تحقق من صلاحيات قاعدة البيانات

3. **الحقول الجديدة لا تظهر:**
   - تأكد من تطبيق Migration على قاعدة البيانات
   - أعد تشغيل التطبيق

---

## ✅ الخلاصة

### الإضافات الرئيسية:
1. ✅ 5 حقول جديدة في جدول المنتسبين
2. ✅ 3 حقول تتبع التواريخ في القرار النهائي
3. ✅ حقول أسباب في الاستشارات والتحاليل
4. ✅ نظام رقم دور تلقائي مع Reset يومي
5. ✅ تحسينات على استقرار الاتصال بقاعدة البيانات

### المحذوفات:
1. ❌ حقل السمع من الفحص الباطني
2. ❌ حقل الطبيب المشار إليه من الاستشارات

---

**تم إعداد هذا التوثيق بتاريخ: 17 نوفمبر 2025**

**الإصدار:** v2.0.0

