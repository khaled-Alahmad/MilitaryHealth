using Application.Abstractions;
using Application.DTOs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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
        
        // Set QuestPDF license (Community license for free use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<List<RecruitmentExportDto>> GetPendingExportsAsync()
    {
        // First, get the data from database
        var pendingData = await _context.FinalDecisions
            .Include(fd => fd.Result)
            .Where(fd => !fd.IsExportedToRecruitment)
            .Join(_context.Applicants.Include(a => a.MaritalStatus),
                fd => fd.ApplicantFileNumber,
                app => app.FileNumber,
                (fd, app) => new { FinalDecision = fd, Applicant = app })
            .OrderBy(x => x.Applicant.CreatedAt)
            .ToListAsync();

        // Then, apply sequence numbering in memory
        var pendingExports = pendingData.Select((x, index) => new RecruitmentExportDto
        {
            SequenceNumber = index + 1,
            FileNumber = x.Applicant.FileNumber,
            AssociateNumber = x.Applicant.AssociateNumber,
            RecruitmentCenter = x.Applicant.RecruitmentCenter,
           
            Result = x.FinalDecision.Result?.Description,
            SupervisorEvaluationDate = x.FinalDecision.SupervisorLastModifiedAt ?? x.FinalDecision.SupervisorAddedAt,
            Reason = x.FinalDecision.Reason
        }).ToList();

        return pendingExports;
    }

    public async Task<ExportToRecruitmentResponse> ExportToRecruitmentAsync(ExportToRecruitmentRequest request)
    {
        try
        {
            // Get decisions to export
            IQueryable<FinalDecision> query = _context.FinalDecisions
                .Include(fd => fd.Result);

            if (request.ExportAll)
            {
                // Export all non-exported
                query = query.Where(fd => !fd.IsExportedToRecruitment);
            }
            else if (request.DecisionIds != null && request.DecisionIds.Any())
            {
                // Export specific decisions
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

            // Get applicant data with marital status
            var applicantFileNumbers = decisionsToExport.Select(d => d.ApplicantFileNumber).ToList();
            var applicantsData = await _context.Applicants
                .Include(a => a.MaritalStatus)
                .Where(a => applicantFileNumbers.Contains(a.FileNumber))
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            // Prepare export data
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

            // Generate protected PDF
            var pdfData = GenerateProtectedPdf(exportData);

            // Mark decisions as exported
            var exportDate = DateTime.Now;
            foreach (var decision in decisionsToExport)
            {
                decision.IsExportedToRecruitment = true;
                decision.ExportedAt = exportDate;
            }

            await _context.SaveChangesAsync();

            var fileName = $"Recruitment_Export_{exportDate:yyyyMMdd_HHmmss}.pdf";

            return new ExportToRecruitmentResponse
            {
                Success = true,
                Message = $"تم تصدير {exportData.Count} منتسب بنجاح",
                ExportedCount = exportData.Count,
                PdfFileData = pdfData,
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

    private byte[] GenerateProtectedPdf(List<RecruitmentExportDto> data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); // عرضي للحصول على مساحة أكبر
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                // Header
                page.Header().Element(ComposeHeader);

                // Content - Table
                page.Content().Element(content => ComposeContent(content, data));

                // Footer
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("تم الإنشاء بتاريخ: ");
                    text.Span(DateTime.Now.ToString("yyyy/MM/dd - HH:mm"));
                    text.Span(" | ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        // Generate PDF with metadata
        var pdfBytes = document.GeneratePdf();

        return pdfBytes;
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().AlignCenter().Text("الجمهورية العربية السورية")
                .FontSize(14).Bold().FontFamily("Arial");
            
            column.Item().AlignCenter().Text("وزارة الدفاع - ادارة الخدمات الطبية")
                .FontSize(12).SemiBold().FontFamily("Arial");
            
         
            
            column.Item().PaddingVertical(5).LineHorizontal(1);
        });
    }

    private void ComposeContent(IContainer container, List<RecruitmentExportDto> data)
    {
        container.Table(table =>
        {
            // Define columns
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);  // التعداد
                columns.ConstantColumn(80);  // رقم الاستمارة
                columns.RelativeColumn(2);   // رقم التجنيد
                columns.RelativeColumn(1);   // المركز
                columns.ConstantColumn(60);  // النتيجة
                columns.ConstantColumn(70);  // تاريخ التقييم
                columns.RelativeColumn(1.5f); // السبب
            });

            // Header row
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).AlignCenter().Text("التعداد").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("رقم الاستمارة").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("رقم التجنيد").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("مركز التجنيد").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("النتيجة").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("تاريخ التقييم").Bold();
                header.Cell().Element(CellStyle).AlignCenter().Text("السبب").Bold();

                static IContainer CellStyle(IContainer container)
                {
                    return container
                        .Border(1)
                        .BorderColor(Colors.Grey.Darken2)
                        .Background(Colors.Grey.Lighten3)
                        .PaddingVertical(5)
                        .PaddingHorizontal(3);
                }
            });

            // Data rows
            foreach (var item in data)
            {
                table.Cell().Element(CellStyle).AlignCenter().Text(item.SequenceNumber.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text(item.FileNumber ?? "-");
                table.Cell().Element(CellStyle).AlignRight().Text(item.AssociateNumber ?? "-");
                table.Cell().Element(CellStyle).AlignRight().Text(item.RecruitmentCenter ?? "-");
                table.Cell().Element(CellStyle).AlignCenter().Text(item.Result ?? "-");
                table.Cell().Element(CellStyle).AlignCenter()
                    .Text(item.SupervisorEvaluationDate?.ToString("yyyy/MM/dd") ?? "-");
                table.Cell().Element(CellStyle).AlignRight().Text(item.Reason ?? "-");

                static IContainer CellStyle(IContainer container)
                {
                    return container
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .PaddingVertical(3)
                        .PaddingHorizontal(3);
                }
            }
        });
    }
}

