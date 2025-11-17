using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class InvestigationDto
{
    public int InvestigationID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    public string? Type { get; set; }

    public string? InvestigationReason { get; set; }

    public string? Result { get; set; }

    public string? Attachment { get; set; }

    public string? Status { get; set; }

    public int DoctorID { get; set; }

    public virtual DoctorDto Doctor { get; set; } = null!;
}
