using Application.Abstractions;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecruitmentExportController : ControllerBase
{
    private readonly IRecruitmentExportService _exportService;

    public RecruitmentExportController(IRecruitmentExportService exportService)
    {
        _exportService = exportService;
    }

    /// <summary>
    /// Get list of pending exports (not yet exported to recruitment)
    /// الحصول على قائمة المنتسبين الجاهزين للتصدير
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<RecruitmentExportDto>>> GetPendingExports()
    {
        try
        {
            var pendingExports = await _exportService.GetPendingExportsAsync();
            return Ok(pendingExports);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"حدث خطأ: {ex.Message}" });
        }
    }

    /// <summary>
    /// Export applicants to recruitment and generate protected PDF
    /// تصدير المنتسبين لمركز التجنيد وإنشاء PDF محمي
    /// </summary>
    /// <param name="request">
    /// Export request with:
    /// - DecisionIds: list of specific decision IDs to export
    /// - ExportAll: true to export all non-exported decisions
    /// </param>
    /// <returns>PDF file with exported data</returns>
    [HttpPost("export")]
    public async Task<IActionResult> ExportToRecruitment([FromBody] ExportToRecruitmentRequest request)
    {
        try
        {
            var result = await _exportService.ExportToRecruitmentAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            // Return PDF file
            return File(
                result.PdfFileData!,
                "application/pdf",
                result.FileName
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"حدث خطأ: {ex.Message}" });
        }
    }

    /// <summary>
    /// Export all non-exported applicants to recruitment
    /// تصدير جميع المنتسبين غير المُصدّرين
    /// </summary>
    [HttpPost("export-all")]
    public async Task<IActionResult> ExportAllToRecruitment()
    {
        try
        {
            var request = new ExportToRecruitmentRequest
            {
                ExportAll = true
            };

            var result = await _exportService.ExportToRecruitmentAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            // Return PDF file
            return File(
                result.PdfFileData!,
                "application/pdf",
                result.FileName
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"حدث خطأ: {ex.Message}" });
        }
    }
}

