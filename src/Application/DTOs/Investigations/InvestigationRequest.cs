using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class InvestigationRequest
{

    public string ApplicantFileNumber { get; set; } = null!;

    public string? Type { get; set; }

    public string? InvestigationReason { get; set; }

    public string? Result { get; set; }

    public string? Attachment { get; set; }

    public string? Status { get; set; }

    public int DoctorID { get; set; }

}
