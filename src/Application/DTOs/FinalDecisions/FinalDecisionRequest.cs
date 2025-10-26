using System;
using System.Collections.Generic;

namespace Infrastructure.Persistence.Models;

public partial class FinalDecisionRequest
{


    public int OrthopedicExamID { get; set; }

    public int SurgicalExamID { get; set; }

    public int InternalExamID { get; set; }

    public int EyeExamID { get; set; }
   public int EarClinicID { get; set; } 


    public string ApplicantFileNumber { get; set; } = null!;

    public int? ResultID { get; set; }

    public string? Reason { get; set; }

    public string? PostponeDuration { get; set; }

    public DateOnly DecisionDate { get; set; }





}
