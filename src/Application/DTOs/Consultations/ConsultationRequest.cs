using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class ConsultationRequest
{

    public int? DoctorID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    public string? ConsultationType { get; set; }

    public string? ReferralReason { get; set; }

    public string? Result { get; set; }

    public string? Attachment { get; set; }

}
