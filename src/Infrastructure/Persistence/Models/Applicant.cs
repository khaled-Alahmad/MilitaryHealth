using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class Applicant
{
    public int ApplicantID { get; set; }

    public string FileNumber { get; set; } = null!;

    public string AssociateNumber { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? MotherName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? RecruitmentCenter { get; set; }

    public string? BloodType { get; set; }

    public int? MaritalStatusID { get; set; }

    public string? Job { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public decimal? BMI { get; set; }

    public string? BloodPressure { get; set; }

    public int? Pulse { get; set; }

    public bool? Tattoo { get; set; }

    public string? DistinctiveMarks { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? QueueNumber { get; set; }

    public virtual ICollection<Archive> ArchiveApplicantFileNumberNavigations { get; set; } = new List<Archive>();

    public virtual ICollection<Archive> ArchiveApplicants { get; set; } = new List<Archive>();

    public virtual MaritalStatus? MaritalStatus { get; set; }
}
