using System;

namespace Application.DTOs;

/// <summary>
/// DTO for exporting applicant data to recruitment
/// البيانات المُصدّرة لمركز التجنيد
/// </summary>
public class RecruitmentExportDto
{
    /// <summary>تعداد - رقم تسلسلي</summary>
    public int SequenceNumber { get; set; }

    /// <summary>رقم الاستمارة</summary>
    public string FileNumber { get; set; } = null!;

    public string? AssociateNumber { get; set; }

    /// <summary>الاسم الكامل</summary>
    public string? FullName { get; set; }

    /// <summary>اسم الأم</summary>
    public string? MotherName { get; set; }

    /// <summary>الحالة الاجتماعية (وصف)</summary>
    public string? MaritalStatus { get; set; }

    /// <summary>تاريخ الميلاد</summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>زمرة الدم</summary>
    public string? BloodType { get; set; }

    /// <summary>اسم مركز التجنيد</summary>
    public string? RecruitmentCenter { get; set; }

    /// <summary>النتيجة (لائق/غير لائق)</summary>
    public string? Result { get; set; }

    /// <summary>تاريخ تقييم المشرف</summary>
    public DateTime? SupervisorEvaluationDate { get; set; }

    /// <summary>السبب</summary>
    public string? Reason { get; set; }

    /// <summary>معرف القرار النهائي (للاستخدام في التصدير)</summary>
    public int? DecisionID { get; set; }

    /// <summary>تاريخ التصدير للتجنيد (للعناصر المُصدَّرة فقط)</summary>
    public DateTime? ExportedAt { get; set; }
}

/// <summary>
/// Request DTO for exporting applicants to recruitment
/// طلب تصدير البيانات
/// </summary>
public class ExportToRecruitmentRequest
{
    /// <summary>
    /// List of Decision IDs to export. If null or empty, export all non-exported.
    /// قائمة معرّفات القرارات للتصدير. إذا كانت فارغة، يتم تصدير كل ما لم يُصدّر.
    /// </summary>
    public List<int>? DecisionIds { get; set; }

    /// <summary>
    /// If true, export all non-exported decisions
    /// إذا كانت true، يتم تصدير كل القرارات غير المُصدّرة
    /// </summary>
    public bool ExportAll { get; set; }
}

/// <summary>
/// Response after export
/// رد بعد التصدير
/// </summary>
public class ExportToRecruitmentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = null!;
    public int ExportedCount { get; set; }
    /// <summary>Excel file data (.xlsx, UTF-8 / Arabic supported)</summary>
    public byte[]? FileData { get; set; }
    public string? FileName { get; set; }
}

