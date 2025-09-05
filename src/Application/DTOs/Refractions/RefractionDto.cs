using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class RefractionDto
{
    public int RefractionID { get; set; }

    public int RefractionTypeID { get; set; }

    public decimal? RefractionValue { get; set; }

    public bool? IsLeft { get; set; }

    public int? EyeExamID { get; set; }

    public virtual EyeExamDto? EyeExam { get; set; }

    public virtual RefractionTypeDto RefractionType { get; set; } = null!;
}
