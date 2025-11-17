// Application/DTOs/Applicants/ApplicantDto.cs
using System.ComponentModel.DataAnnotations;
namespace Application.DTOs;

public class ApplicantDto
{
    public int? ApplicantID { get; set; }
    public string? FileNumber { get; set; } = null!;
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
    [Required(ErrorMessage = "Height is required")]

    public decimal? Height { get; set; }
    [Required(ErrorMessage = "Weight is required")]

    public decimal? Weight { get; set; }
    [Required(ErrorMessage = "BMI is required")]

    public decimal? BMI { get; set; }
    [Required(ErrorMessage = "Blood Pressure is required")]

    public string? BloodPressure { get; set; }
    [Required(ErrorMessage = "Pulse is required")]

    public int? Pulse { get; set; }
    [Required(ErrorMessage = "Tattoo is required")]

    public bool? Tattoo { get; set; }
    public string AssociateNumber { get; set; }

    public string? DistinctiveMarks { get; set; }
    public DateTime? CreatedAt { get; set; }

    public int? QueueNumber { get; set; }

    public MaritalStatusDto? MaritalStatus { get; set; }


}

