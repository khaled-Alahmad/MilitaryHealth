using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class Result
{
    public int ResultID { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<EarClinicExam> EarClinicExams { get; set; } = new List<EarClinicExam>();

    public virtual ICollection<EyeExam> EyeExams { get; set; } = new List<EyeExam>();

    public virtual ICollection<FinalDecision> FinalDecisions { get; set; } = new List<FinalDecision>();

    public virtual ICollection<InternalExam> InternalExams { get; set; } = new List<InternalExam>();

    public virtual ICollection<OrthopedicExam> OrthopedicExams { get; set; } = new List<OrthopedicExam>();

    public virtual ICollection<SurgicalExam> SurgicalExams { get; set; } = new List<SurgicalExam>();
}
