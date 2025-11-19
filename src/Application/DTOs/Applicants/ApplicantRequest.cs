// Application/DTOs/Applicants/ApplicantDto.cs
using Application.DTOs;
using System.ComponentModel.DataAnnotations;

public class ApplicantRequest
{
    public int? ApplicantID { get; set; }

    [Required(ErrorMessage = "FullName is required")]

    public string FullName { get; set; } = null!;

    public string? MotherName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? RecruitmentCenter { get; set; }

    public string? BloodType { get; set; }
    [Required(ErrorMessage = "Marital Status is required")]

    public int? MaritalStatusID { get; set; }
    [Required(ErrorMessage = "Username is required")]

    public string? Job { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public decimal? BMI { get; set; }

    public string? BloodPressure { get; set; }

    public int? Pulse { get; set; }
    [Required(ErrorMessage = "Tattoo is required")]

    public bool? Tattoo { get; set; }
    public string AssociateNumber { get; set; }

    public string? DistinctiveMarks { get; set; }

    public int? QueueNumber { get; set; }

    public MaritalStatusDto? MaritalStatus { get; set; }


}

