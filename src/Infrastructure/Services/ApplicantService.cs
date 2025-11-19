using Application.DTOs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicantService : IApplicantService
{
    private readonly AppDbContext _db;

    public ApplicantService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ApplicantDetailsDto?> GetApplicantDetailsAsync(string id, CancellationToken ct = default)
    {
        var applicant = await _db.Applicants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FileNumber == id, ct);

        if (applicant == null)
            return null;

        var eyeExam = await _db.EyeExams.AsNoTracking()
            .Where(e => e.ApplicantFileNumber == id)
            .Select(e => new EyeExamDto
            {
                EyeExamID = e.EyeExamID,
                ApplicantFileNumber = e.ApplicantFileNumber,
                DoctorID = e.DoctorID,
                Vision = e.Vision,
                ColorTest = e.ColorTest,
                WorstRefractionRight = e.WorstRefractionRight,
                WorstRefractionLeft = e.WorstRefractionLeft,
                VisionLeft = e.VisionLeft,
                ColorTestLeft = e.ColorTestLeft,

                Refractions = e.Refractions!.Select(r => new RefractionDto
                {
                    RefractionID = r.RefractionID,
                    EyeExamID = r.EyeExamID,
                    IsLeft = r.IsLeft,
                    RefractionTypeID = r.RefractionTypeID,
                    RefractionValue = r.RefractionValue
                }).ToList(),

                OtherDiseases = e.OtherDiseases,
                ResultID = e.ResultID,
                Reason = e.Reason
            })
            .FirstOrDefaultAsync(ct);

        var internalExam = await _db.InternalExams.AsNoTracking()
            .Where(e => e.ApplicantFileNumber == id)
            .Select(e => new InternalExamDto
            {
                InternalExamID = e.InternalExamID,
                ApplicantFileNumber = e.ApplicantFileNumber,
                DoctorID = e.DoctorID,
                Heart = e.Heart,
                Respiratory = e.Respiratory,
                Digestive = e.Digestive,
                Endocrine = e.Endocrine,
                Neurology = e.Neurology,
                Blood = e.Blood,
                Joints = e.Joints,
                Kidney = e.Kidney,
                Skin = e.Skin,
                ResultID = e.ResultID,
                Reason = e.Reason
            })
            .FirstOrDefaultAsync(ct);

        var orthopedicExam = await _db.OrthopedicExams.AsNoTracking()
            .Where(e => e.ApplicantFileNumber == id)
            .Select(e => new OrthopedicExamDto
            {
                OrthopedicExamID = e.OrthopedicExamID,
                ApplicantFileNumber = e.ApplicantFileNumber,
                DoctorID = e.DoctorID,
                Musculoskeletal = e.Musculoskeletal,
                NeurologicalSurgery = e.NeurologicalSurgery,
                ResultID = e.ResultID,
                Reason = e.Reason
            })
            .FirstOrDefaultAsync(ct);
        var earClinicExam = await _db.EarClinicExams.AsNoTracking()
            .Where(e => e.ApplicantFileNumber == id)
            .Select(e => new EarClinicExamDto
            {
                
                EarClinicID = e.EarClinicID,
                ApplicantFileNumber = e.ApplicantFileNumber,
                DoctorID = e.DoctorID,
                RightEar = e.RightEar,
                LeftEar = e.LeftEar,
                ResultID = e.ResultID,

                Reason = e.Reason,
                Doctor = new DoctorDto
                {
                    DoctorID = e.Doctor!.DoctorID,
                    FullName = e.Doctor.FullName,
                    SpecializationID = e.Doctor.SpecializationID,
                    Code = e.Doctor.Code,
                },
                isLeftHugeMates = e.isLeftHugeMates,
                isRightHugeMates = e.isRightHugeMates,
                LeftHearing = e.LeftHearing,
                LeftNose = e.LeftNose,
                LeftString = e.LeftString,
                LeftTympanicMembrane = e.LeftTympanicMembrane,
                Mouth = e.Mouth,
                OtherDiseases = e.OtherDiseases,
                
                Resonators = e.Resonators,
                RightHearing = e.RightHearing,
                RightNose = e.RightNose,
                RightString = e.RightString,
                RightTympanicMembrane = e.RightTympanicMembrane,
                LeftWhisperTest = e.LeftWhisperTest,
                RightWhisperTest = e.RightWhisperTest,
                Result = new ResultDto
                {
                    ResultID = e.Result!.ResultID,
                    Description = e.Result.Description,
                }
            })
            .FirstOrDefaultAsync(ct);
        var finalDecision = await _db.FinalDecisions.AsNoTracking()
            .Where(e => e.ApplicantFileNumber == id)
            .Select(e => new FinalDecisionDto
            {
                ApplicantFileNumber = e.ApplicantFileNumber,
                DecisionID = e.DecisionID,
                InternalExamID = e.InternalExamID,
                EyeExamID = e.EyeExamID,
                OrthopedicExamID = e.OrthopedicExamID,
                SurgicalExamID = e.SurgicalExamID,
                ResultID = e.ResultID,
                Reason = e.Reason,
                PostponeDuration = e.PostponeDuration,
                DecisionDate = e.DecisionDate,
                Result = new Application.DTOs.EyeExams.ResultDto
                {
                    ResultID = e.Result!.ResultID,
                    Description = e.Result.Description,
                },
            })
            .FirstOrDefaultAsync(ct);
        var surgicalExam = await _db.SurgicalExams.AsNoTracking()
            .Where(e => e.ApplicantFileNumber == id)
            .Select(e => new SurgicalExamDto
            {
                SurgicalExamID = e.SurgicalExamID,
                ApplicantFileNumber = e.ApplicantFileNumber,
                DoctorID = e.DoctorID,
                GeneralSurgery = e.GeneralSurgery,
                UrinarySurgery = e.UrinarySurgery,
                VascularSurgery = e.VascularSurgery,
                ThoracicSurgery = e.ThoracicSurgery,
                ResultID = e.ResultID,
                Reason = e.Reason
            })
            .FirstOrDefaultAsync(ct);
        var investigation = await _db.Investigations.AsNoTracking().Where(w => w.ApplicantFileNumber == id).Select(e =>
            new InvestigationDto
            {
                ApplicantFileNumber = e.ApplicantFileNumber,
                Attachment = e.Attachment,
                DoctorID = e.DoctorID,
                InvestigationID = e.InvestigationID,
                Status = e.Status,
                Result = e.Result,
                Type = e.Type
            }).FirstOrDefaultAsync(ct);
        var consultation = await _db.Consultations.AsNoTracking().Where(e => e.ApplicantFileNumber == id
            )
            .Select(e => new ConsultationDto
            {
                ApplicantFileNumber = e.ApplicantFileNumber,
                ConsultationID = e.ConsultationID,
                Attachment = e.Attachment,
                ConsultationType = e.ConsultationType,
                DoctorID = e.DoctorID,
                ReferralReason = e.ReferralReason,
                Result = e.Result
            })
            .FirstOrDefaultAsync(ct);
        return new ApplicantDetailsDto
        {
            ApplicantID = applicant.ApplicantID,
            FileNumber = applicant.FileNumber,
            FullName = applicant.FullName,
            MaritalStatusID = applicant.MaritalStatusID,
            Job = applicant.Job,
            Height = applicant.Height,
            Weight = applicant.Weight,
            BMI = applicant.BMI,
            BloodPressure = applicant.BloodPressure,
            Pulse = applicant.Pulse,
            Tattoo = applicant.Tattoo,
            DistinctiveMarks = applicant.DistinctiveMarks,
            CreatedAt = applicant.CreatedAt,
            Investigation = investigation,
            Consultation = consultation,
            EyeExam = eyeExam,
            InternalExam = internalExam,
            OrthopedicExamDto = orthopedicExam,
            SurgicalExam = surgicalExam,
            finalDecision = finalDecision,
            AssociateNumber = applicant.AssociateNumber,
            EarClinic = earClinicExam
        };
    }

    public async Task<ApplicantDetailsDto?> GetApplicantAsync(string id, CancellationToken ct = default)
    {
        var applicant = await _db.Applicants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FileNumber == id, ct);

        if (applicant == null)
            return null;
        var maritalStatus = await _db.MaritalStatuses.AsNoTracking()
            .Where(e => e.MaritalStatusID == applicant.MaritalStatusID).Select(w => new MaritalStatusDto
            {
                MaritalStatusID = w.MaritalStatusID,
                Description = w.Description,
            })
            .FirstOrDefaultAsync(ct);


        return new ApplicantDetailsDto
        {
            ApplicantID = applicant.ApplicantID,
            FileNumber = applicant.FileNumber,
            FullName = applicant.FullName,
            MaritalStatusID = applicant.MaritalStatusID,
            Job = applicant.Job,
            Height = applicant.Height,
            Weight = applicant.Weight,
            BMI = applicant.BMI,
            BloodPressure = applicant.BloodPressure,
            Pulse = applicant.Pulse,
            Tattoo = applicant.Tattoo,
            DistinctiveMarks = applicant.DistinctiveMarks,
            CreatedAt = applicant.CreatedAt,
            MaritalStatus = maritalStatus,
        };
    }

    public async Task<ApplicantsStatisticsDto> GetStatisticsAsync(CancellationToken ct)
    {
        var total = await _db.FinalDecisions.CountAsync(ct);
        var accepted = await _db.FinalDecisions.CountAsync(a => a.ResultID == 1, ct);
        var rejected = await _db.FinalDecisions.CountAsync(a => a.ResultID == 3, ct);
        var pending = await _db.FinalDecisions.CountAsync(a => a.ResultID == 2, ct);

        return new ApplicantsStatisticsDto
        {
            Total = total,
            Accepted = accepted,
            Rejected = rejected,
            Pending = pending
        };
    }
}
