using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class FinalDecision
{
    public int DecisionID { get; set; }

    public int OrthopedicExamID { get; set; }

    public int SurgicalExamID { get; set; }

    public int InternalExamID { get; set; }

    public int EyeExamID { get; set; }

    public int EarClinicID { get; set; }

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

    public virtual EarClinicExam EarClinic { get; set; } = null!;

    public virtual EyeExam EyeExam { get; set; } = null!;

    public virtual InternalExam InternalExam { get; set; } = null!;

    public virtual OrthopedicExam OrthopedicExam { get; set; } = null!;

    public virtual Result? Result { get; set; }

    public virtual SurgicalExam SurgicalExam { get; set; } = null!;
}
