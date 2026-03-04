using Application.Abstractions;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class FinalDecisionHistoryRecorder : IFinalDecisionHistoryRecorder
{
    private readonly AppDbContext _context;

    public FinalDecisionHistoryRecorder(AppDbContext context)
    {
        _context = context;
    }

    public async Task RecordAsync(
        int decisionId,
        string applicantFileNumber,
        int? previousResultId,
        int? newResultId,
        string? reason,
        string? changedBy,
        CancellationToken ct = default)
    {
        string? previousDesc = null;
        string? newDesc = null;
        if (previousResultId.HasValue || newResultId.HasValue)
        {
            var resultIds = new[] { previousResultId, newResultId }.Where(id => id.HasValue).Select(id => id!.Value).ToList();
            var descriptions = await _context.Results
                .AsNoTracking()
                .Where(r => resultIds.Contains(r.ResultID))
                .ToDictionaryAsync(r => r.ResultID, r => r.Description, ct);
            if (previousResultId.HasValue && descriptions.TryGetValue(previousResultId.Value, out var pd))
                previousDesc = pd;
            if (newResultId.HasValue && descriptions.TryGetValue(newResultId.Value, out var nd))
                newDesc = nd;
        }

        var record = new FinalDecisionHistory
        {
            DecisionID = decisionId,
            ApplicantFileNumber = applicantFileNumber,
            PreviousResultID = previousResultId,
            PreviousResultDescription = previousDesc,
            NewResultID = newResultId,
            NewResultDescription = newDesc,
            Reason = reason,
            ChangedAt = DateTime.UtcNow,
            ChangedBy = changedBy
        };

        _context.FinalDecisionHistories.Add(record);
        await _context.SaveChangesAsync(ct);
    }
}
