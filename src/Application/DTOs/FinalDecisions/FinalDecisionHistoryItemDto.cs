namespace Application.DTOs.FinalDecisions;

/// <summary>
/// عنصر من سجل تغيير النتيجة النهائية (كان مرفوض → صار مقبول)
/// </summary>
public class FinalDecisionHistoryItemDto
{
    public int Id { get; set; }
    public int DecisionID { get; set; }
    public string ApplicantFileNumber { get; set; } = null!;
    public int? PreviousResultID { get; set; }
    public string? PreviousResultDescription { get; set; }
    public int? NewResultID { get; set; }
    public string? NewResultDescription { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
}
