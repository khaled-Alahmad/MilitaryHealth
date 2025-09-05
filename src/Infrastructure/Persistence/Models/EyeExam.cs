using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class EyeExam
{
    public int EyeExamID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    public int? DoctorID { get; set; }

    public string? Vision { get; set; }

    public string? VisionLeft { get; set; }

    public string? ColorTestLeft { get; set; }

    public string? ColorTest { get; set; }

    public string? OtherDiseases { get; set; }

    public int? ResultID { get; set; }

    public string? Reason { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual ICollection<FinalDecision> FinalDecisions { get; set; } = new List<FinalDecision>();

    public virtual ICollection<Refraction> Refractions { get; set; } = new List<Refraction>();

    public virtual Result? Result { get; set; }
}
