using System;
using System.Collections.Generic;
using System.Numerics;

namespace Application.DTOs;

public partial class ConsultationDto
{
    public int ConsultationID { get; set; }

    public int? DoctorID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    public string? ConsultationType { get; set; }

    public string? ReferralReason { get; set; }

    public string? Result { get; set; }

    public string? Attachment { get; set; }

    public virtual DoctorDto Doctor { get; set; } = null!;
}
