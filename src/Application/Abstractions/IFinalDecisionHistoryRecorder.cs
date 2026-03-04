using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions;

/// <summary>
/// تسجيل سجل تغيير النتيجة النهائية (كان مرفوض → صار مقبول)
/// </summary>
public interface IFinalDecisionHistoryRecorder
{
    Task RecordAsync(
        int decisionId,
        string applicantFileNumber,
        int? previousResultId,
        int? newResultId,
        string? reason,
        string? changedBy,
        CancellationToken ct = default);
}
