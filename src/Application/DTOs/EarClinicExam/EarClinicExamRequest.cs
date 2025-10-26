using Application.DTOs.EyeExams;
using System;
using System.Collections.Generic;

namespace Application.DTOs;

public partial class EarClinicExamRequest
{
    public int EarClinicID { get; set; }

    public string? ApplicantFileNumber { get; set; }

    public int? DoctorID { get; set; }

    public string? RightEar { get; set; }

    public string? LeftEar { get; set; }

    public string? RightTympanicMembrane { get; set; }

    public string? LeftTympanicMembrane { get; set; }

    public string? RightHearing { get; set; }

    public string? LeftHearing { get; set; }

    public string? Resonators { get; set; }

    public string? RightWhisperTest { get; set; }

    public string? LeftWhisperTest { get; set; }

    public string? RightNose { get; set; }

    public string? LeftNose { get; set; }

    public bool? isRightHugeMates { get; set; }

    public bool? isLeftHugeMates { get; set; }

    public string? RightString { get; set; }

    public string? LeftString { get; set; }

    public string? Mouth { get; set; }

    public string? OtherDiseases { get; set; }

    public int? ResultID { get; set; }

    public string? Reason { get; set; }



}
