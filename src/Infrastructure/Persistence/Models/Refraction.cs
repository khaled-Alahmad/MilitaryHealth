using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class Refraction
{
    public int RefractionID { get; set; }

    public int RefractionTypeID { get; set; }

    public decimal? RefractionValue { get; set; }

    public bool? IsLeft { get; set; }

    public int? EyeExamID { get; set; }

    public virtual EyeExam? EyeExam { get; set; }

    public virtual RefractionType RefractionType { get; set; } = null!;
}
