using Application.Abstractions;
using Application.DTOs;
using ClosedXML.Excel;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Services;

public class RecruitmentExportService : IRecruitmentExportService
{
    private readonly AppDbContext _context;

    public RecruitmentExportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecruitmentExportDto>> GetPendingExportsAsync()
    {
        var pendingData = await _context.FinalDecisions
            .Include(fd => fd.Result)
            .Where(fd => !fd.IsExportedToRecruitment)
            .Join(_context.Applicants.Include(a => a.MaritalStatus),
                fd => fd.ApplicantFileNumber,
                app => app.FileNumber,
                (fd, app) => new { FinalDecision = fd, Applicant = app })
            .OrderBy(x => x.Applicant.CreatedAt)
            .ToListAsync();

        var pendingExports = pendingData.Select((x, index) => new RecruitmentExportDto
        {
            SequenceNumber = index + 1,
            FileNumber = x.Applicant.FileNumber,
            AssociateNumber = x.Applicant.AssociateNumber,
            FullName = x.Applicant.FullName,
            MotherName = x.Applicant.MotherName,
            MaritalStatus = x.Applicant.MaritalStatus?.Description,
            DateOfBirth = x.Applicant.DateOfBirth,
            BloodType = x.Applicant.BloodType,
            RecruitmentCenter = x.Applicant.RecruitmentCenter,
            Result = x.FinalDecision.Result?.Description,
            SupervisorEvaluationDate = x.FinalDecision.SupervisorLastModifiedAt ?? x.FinalDecision.SupervisorAddedAt,
            Reason = x.FinalDecision.Reason,
            DecisionID = x.FinalDecision.DecisionID
        }).ToList();

        return pendingExports;
    }

    public async Task<List<RecruitmentExportDto>> GetExportedToRecruitmentAsync()
    {
        var exportedData = await _context.FinalDecisions
            .Include(fd => fd.Result)
            .Where(fd => fd.IsExportedToRecruitment)
            .Join(_context.Applicants.Include(a => a.MaritalStatus),
                fd => fd.ApplicantFileNumber,
                app => app.FileNumber,
                (fd, app) => new { FinalDecision = fd, Applicant = app })
            .OrderByDescending(x => x.FinalDecision.ExportedAt)
            .ToListAsync();

        var list = exportedData.Select((x, index) => new RecruitmentExportDto
        {
            SequenceNumber = index + 1,
            FileNumber = x.Applicant.FileNumber,
            AssociateNumber = x.Applicant.AssociateNumber,
            FullName = x.Applicant.FullName,
            MotherName = x.Applicant.MotherName,
            MaritalStatus = x.Applicant.MaritalStatus?.Description,
            DateOfBirth = x.Applicant.DateOfBirth,
            BloodType = x.Applicant.BloodType,
            RecruitmentCenter = x.Applicant.RecruitmentCenter,
            Result = x.FinalDecision.Result?.Description,
            SupervisorEvaluationDate = x.FinalDecision.SupervisorLastModifiedAt ?? x.FinalDecision.SupervisorAddedAt,
            Reason = x.FinalDecision.Reason,
            DecisionID = x.FinalDecision.DecisionID,
            ExportedAt = x.FinalDecision.ExportedAt
        }).ToList();

        return list;
    }

    public async Task<ExportToRecruitmentResponse> ExportToRecruitmentAsync(ExportToRecruitmentRequest request)
    {
        try
        {
            IQueryable<FinalDecision> query = _context.FinalDecisions
                .Include(fd => fd.Result);

            if (request.ExportAll)
            {
                query = query.Where(fd => !fd.IsExportedToRecruitment);
            }
            else if (request.DecisionIds != null && request.DecisionIds.Any())
            {
                query = query.Where(fd => request.DecisionIds.Contains(fd.DecisionID));
            }
            else
            {
                return new ExportToRecruitmentResponse
                {
                    Success = false,
                    Message = "يجب تحديد قرارات للتصدير أو اختيار تصدير الكل",
                    ExportedCount = 0
                };
            }

            var decisionsToExport = await query.ToListAsync();

            if (!decisionsToExport.Any())
            {
                return new ExportToRecruitmentResponse
                {
                    Success = false,
                    Message = "لا توجد قرارات للتصدير",
                    ExportedCount = 0
                };
            }

            var applicantFileNumbers = decisionsToExport.Select(d => d.ApplicantFileNumber).ToList();
            var applicantsData = await _context.Applicants
                .Include(a => a.MaritalStatus)
                .Where(a => applicantFileNumbers.Contains(a.FileNumber))
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            var exportData = new List<RecruitmentExportDto>();
            int sequenceNumber = 1;

            foreach (var applicant in applicantsData)
            {
                var decision = decisionsToExport.FirstOrDefault(d => d.ApplicantFileNumber == applicant.FileNumber);
                if (decision != null)
                {
                    exportData.Add(new RecruitmentExportDto
                    {
                        SequenceNumber = sequenceNumber++,
                        FileNumber = applicant.FileNumber,
                        RecruitmentCenter = applicant.RecruitmentCenter,
                        Result = decision.Result?.Description,
                        AssociateNumber = applicant.AssociateNumber,
                        SupervisorEvaluationDate = decision.SupervisorLastModifiedAt ?? decision.SupervisorAddedAt,
                        Reason = decision.Reason
                    });
                }
            }

            var excelData = GenerateExcel(exportData);

            var exportDate = DateTime.Now;
            foreach (var decision in decisionsToExport)
            {
                decision.IsExportedToRecruitment = true;
                decision.ExportedAt = exportDate;
            }

            await _context.SaveChangesAsync();

            var fileName = $"Recruitment_Export_{exportDate:yyyyMMdd_HHmmss}.xlsx";

            return new ExportToRecruitmentResponse
            {
                Success = true,
                Message = $"تم تصدير {exportData.Count} منتسب بنجاح",
                ExportedCount = exportData.Count,
                FileData = excelData,
                FileName = fileName
            };
        }
        catch (Exception ex)
        {
            return new ExportToRecruitmentResponse
            {
                Success = false,
                Message = $"حدث خطأ أثناء التصدير: {ex.Message}",
                ExportedCount = 0
            };
        }
    }

    /// <summary>
    /// Generates Excel file with UTF-8 / Arabic support (RTL sheet).
    /// </summary>
    private static byte[] GenerateExcel(List<RecruitmentExportDto> data)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("تصدير للتجنيد");

        // دعم العربية: اتجاه الورقة من اليمين لليسار
        ws.RightToLeft = true;

        // عنوان التقرير
        ws.Cell(1, 1).Value = "الجمهورية العربية السورية";
        ws.Range(1, 1, 1, 7).Merge().Style.Font.SetBold().Font.SetFontSize(14).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
        ws.Cell(2, 1).Value = "وزارة الدفاع - ادارة الخدمات الطبية";
        ws.Range(2, 1, 2, 7).Merge().Style.Font.SetBold().Font.SetFontSize(12).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        int row = 4;

        // رأس الجدول
        var headers = new[] { "التعداد", "رقم الاستمارة", "رقم التجنيد", "مركز التجنيد", "النتيجة", "تاريخ التقييم", "السبب" };
        for (int col = 1; col <= headers.Length; col++)
        {
            ws.Cell(row, col).Value = headers[col - 1];
            ws.Cell(row, col).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.LightGray);
        }
        row++;

        // صفوف البيانات
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.SequenceNumber;
            ws.Cell(row, 2).Value = item.FileNumber ?? "-";
            ws.Cell(row, 3).Value = item.AssociateNumber ?? "-";
            ws.Cell(row, 4).Value = item.RecruitmentCenter ?? "-";
            ws.Cell(row, 5).Value = item.Result ?? "-";
            ws.Cell(row, 6).Value = item.SupervisorEvaluationDate?.ToString("yyyy/MM/dd") ?? "-";
            ws.Cell(row, 7).Value = item.Reason ?? "-";
            row++;
        }

        // عرض الأعمدة تلقائياً
        ws.Columns().AdjustToContents();

        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
