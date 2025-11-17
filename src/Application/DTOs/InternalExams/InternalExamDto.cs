using Application.DTOs.EyeExams;
using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class InternalExamDto
{
    public int InternalExamID { get; set; }

    public string ApplicantFileNumber { get; set; } = null!;

    public int? DoctorID { get; set; }

    public string? Heart { get; set; }

    public string? Respiratory { get; set; }

    public string? Digestive { get; set; }

    public string? Endocrine { get; set; }

    public string? Neurology { get; set; }

    public string? Blood { get; set; }

    public string? Joints { get; set; }

    public string? Kidney { get; set; }

    public string? Skin { get; set; }

    public int? ResultID { get; set; }

    public string? Reason { get; set; }

    public virtual DoctorDto? Doctor { get; set; }

    public virtual Application.DTOs.EyeExams.ResultDto? Result { get; set; }
}
