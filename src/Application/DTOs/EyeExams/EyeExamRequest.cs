using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class EyeExamRequest
{

    public int EyeExamID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    public int? DoctorID { get; set; }

    public string? Vision { get; set; }
    public string? VisionLeft { get; set; }

    public string? ColorTestLeft { get; set; }
    public string? ColorTest { get; set; }

    //public int? RefractionTypeID { get; set; }

    //public decimal? RefractionValue { get; set; }

    public string? OtherDiseases { get; set; }

    public int? ResultID { get; set; }

    public string? Reason { get; set; }


}
