using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class RefractionRequest
{
    public int RefractionID { get; set; }

    public int RefractionTypeID { get; set; }

    public decimal? RefractionValue { get; set; }

    public bool? IsLeft { get; set; }

    public int? EyeExamID { get; set; }


}
