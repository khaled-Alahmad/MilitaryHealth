using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class Doctor
{
    public int DoctorID { get; set; }

    public string FullName { get; set; } = null!;

    public int? SpecializationID { get; set; }

    public int? ContractTypeID { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ContractType? ContractType { get; set; }

    public virtual ICollection<EarClinicExam> EarClinicExams { get; set; } = new List<EarClinicExam>();

    public virtual ICollection<EyeExam> EyeExams { get; set; } = new List<EyeExam>();

    public virtual ICollection<InternalExam> InternalExams { get; set; } = new List<InternalExam>();

    public virtual ICollection<Investigation> Investigations { get; set; } = new List<Investigation>();

    public virtual ICollection<OrthopedicExam> OrthopedicExams { get; set; } = new List<OrthopedicExam>();

    public virtual Specialization? Specialization { get; set; }

    public virtual ICollection<SurgicalExam> SurgicalExams { get; set; } = new List<SurgicalExam>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
