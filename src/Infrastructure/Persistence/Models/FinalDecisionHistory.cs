using System;

namespace Infrastructure.Persistence.Models;

/// <summary>
/// سجل تغيير النتيجة النهائية (مثلاً: كان مرفوض → صار مقبول)
/// </summary>
public class FinalDecisionHistory
{
    public int Id { get; set; }

    public int DecisionID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    /// <summary>النتيجة السابقة (ResultID)</summary>
    public int? PreviousResultID { get; set; }

    /// <summary>وصف النتيجة السابقة (مثلاً: مرفوض)</summary>
    public string? PreviousResultDescription { get; set; }

    /// <summary>النتيجة الجديدة (ResultID)</summary>
    public int? NewResultID { get; set; }

    /// <summary>وصف النتيجة الجديدة (مثلاً: مقبول)</summary>
    public string? NewResultDescription { get; set; }

    /// <summary>السبب/التوصية عند التعديل</summary>
    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; }

    /// <summary>المستخدم الذي عدّل (اسم المستخدم أو المعرف)</summary>
    public string? ChangedBy { get; set; }
}
