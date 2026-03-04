using Application.DTOs.FinalDecisions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions;

public interface IFinalDecisionHistoryQuery
{
    Task<IReadOnlyList<FinalDecisionHistoryItemDto>> GetByApplicantFileNumberAsync(string applicantFileNumber, CancellationToken ct = default);
}
