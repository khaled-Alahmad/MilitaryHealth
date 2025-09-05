using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class RefractionType
{
    public int RefractionTypeID { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<Refraction> Refractions { get; set; } = new List<Refraction>();
}
