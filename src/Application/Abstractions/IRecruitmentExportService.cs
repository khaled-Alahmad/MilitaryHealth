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
    /// Export applicants to recruitment and generate Excel file (UTF-8 / Arabic supported)
    /// تصدير المنتسبين لمركز التجنيد وإنشاء ملف إكسيل يدعم العربية
    /// </summary>
    /// <param name="request">Export request with decision IDs or export all flag</param>
    /// <returns>Export response with Excel file data</returns>
    Task<ExportToRecruitmentResponse> ExportToRecruitmentAsync(ExportToRecruitmentRequest request);

    /// <summary>
    /// Get list of applicants ready for export (not yet exported)
    /// الحصول على قائمة المنتسبين الجاهزين للتصدير (لم يتم تصديرهم بعد)
    /// </summary>
    Task<List<RecruitmentExportDto>> GetPendingExportsAsync();

    /// <summary>
    /// Get list of applicants already exported to recruitment (لتحميلها مرة ثانية)
    /// الحصول على قائمة الملفات المُصدَّرة سابقاً للتجنيد
    /// </summary>
    Task<List<RecruitmentExportDto>> GetExportedToRecruitmentAsync();
}

