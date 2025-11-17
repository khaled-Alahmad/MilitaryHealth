using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Abstractions;

/// <summary>
/// Service interface for exporting applicant data to recruitment
/// خدمة تصدير بيانات المنتسبين لمركز التجنيد
/// </summary>
public interface IRecruitmentExportService
{
    /// <summary>
    /// Export applicants to recruitment and generate protected PDF
    /// تصدير المنتسبين لمركز التجنيد وإنشاء PDF محمي
    /// </summary>
    /// <param name="request">Export request with decision IDs or export all flag</param>
    /// <returns>Export response with PDF data</returns>
    Task<ExportToRecruitmentResponse> ExportToRecruitmentAsync(ExportToRecruitmentRequest request);

    /// <summary>
    /// Get list of applicants ready for export (not yet exported)
    /// الحصول على قائمة المنتسبين الجاهزين للتصدير (لم يتم تصديرهم بعد)
    /// </summary>
    /// <returns>List of recruitment export DTOs</returns>
    Task<List<RecruitmentExportDto>> GetPendingExportsAsync();
}

