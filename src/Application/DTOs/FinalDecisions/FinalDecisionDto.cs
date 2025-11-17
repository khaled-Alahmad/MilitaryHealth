using Application.DTOs.EyeExams;
using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class FinalDecisionDto
{


    public int DecisionID { get; set; }

    public int OrthopedicExamID { get; set; }

    public int SurgicalExamID { get; set; }

    public int InternalExamID { get; set; }

    public int EyeExamID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    public int? ResultID { get; set; }

    public string? Reason { get; set; }

    public string? PostponeDuration { get; set; }

    public DateOnly DecisionDate { get; set; }

    public DateTime? ReceptionAddedAt { get; set; }

    public DateTime? SupervisorAddedAt { get; set; }

    public DateTime? SupervisorLastModifiedAt { get; set; }

    public bool IsExportedToRecruitment { get; set; }

    public DateTime? ExportedAt { get; set; }

    public virtual EyeExamDto EyeExam { get; set; } = null!;

    public virtual InternalExamDto InternalExam { get; set; } = null!;

    public virtual OrthopedicExamDto OrthopedicExam { get; set; } = null!;

    public virtual Application.DTOs.EyeExams.ResultDto? Result { get; set; }

    public virtual SurgicalExamDto SurgicalExam { get; set; } = null!;


}
