using Application.Abstractions;
using Application.DTOs.FinalDecisions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class FinalDecisionHistoryQuery : IFinalDecisionHistoryQuery
{
    private readonly AppDbContext _context;

    public FinalDecisionHistoryQuery(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FinalDecisionHistoryItemDto>> GetByApplicantFileNumberAsync(string applicantFileNumber, CancellationToken ct = default)
    {
        var list = await _context.FinalDecisionHistories
            .AsNoTracking()
            .Where(h => h.ApplicantFileNumber == applicantFileNumber)
            .OrderByDescending(h => h.ChangedAt)
            .Select(h => new FinalDecisionHistoryItemDto
            {
                Id = h.Id,
                DecisionID = h.DecisionID,
                ApplicantFileNumber = h.ApplicantFileNumber,
                PreviousResultID = h.PreviousResultID,
                PreviousResultDescription = h.PreviousResultDescription,
                NewResultID = h.NewResultID,
                NewResultDescription = h.NewResultDescription,
                Reason = h.Reason,
                ChangedAt = h.ChangedAt,
                ChangedBy = h.ChangedBy
            })
            .ToListAsync(ct);

        return list;
    }
}
