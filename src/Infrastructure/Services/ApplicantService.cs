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
                //RefractionTypeID = e.RefractionTypeID,
                //RefractionValue = e.RefractionValue,
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
                Hearing = e.Hearing,
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
        var investigation = await _db.Investigations.AsNoTracking().Where(w => w.ApplicantFileNumber == id).Select(
            e => new InvestigationDto
            {
                ApplicantFileNumber = e.ApplicantFileNumber,
                Attachment = e.Attachment,
                DoctorID = e.DoctorID,
                InvestigationID = e.InvestigationID,
                Status = e.Status,
                Result = e.Result,
                Type = e.Type
            }).FirstOrDefaultAsync(ct);
        var consultation = await _db.Consultations.AsNoTracking().Where(
            e=>e.ApplicantFileNumber==id
            )
            
            .Select(
            e=>new ConsultationDto
            {
                ApplicantFileNumber=e.ApplicantFileNumber,
                ConsultationID=e.ConsultationID,
                Attachment=e.Attachment,
                ConsultationType= e.ConsultationType,
                DoctorID = e.DoctorID,
                ReferredDoctor = e.ReferredDoctor,
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
            Investigation= investigation,
            Consultation= consultation,
            EyeExam = eyeExam,
            InternalExam = internalExam,
            OrthopedicExamDto = orthopedicExam,
            SurgicalExam = surgicalExam
        };
    }

    public async Task<ApplicantDetailsDto?> GetApplicantAsync(string id, CancellationToken ct = default)
    {
        var applicant = await _db.Applicants
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FileNumber == id, ct);

        if (applicant == null)
            return null;
        var maritalStatus = await _db.MaritalStatuses.AsNoTracking().Where(
            e => e.MaritalStatusID == applicant.MaritalStatusID).
            Select(w=>new MaritalStatusDto
            {
                MaritalStatusID = w.MaritalStatusID,
                Description= w.Description,
            })
            .
            FirstOrDefaultAsync(ct);

       
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
            MaritalStatus= maritalStatus,
     
        };
    }

    public async Task<ApplicantsStatisticsDto> GetStatisticsAsync(CancellationToken ct)
    {
        var total = await _db.FinalDecisions.CountAsync(ct);
        var accepted = await _db.FinalDecisions.CountAsync(a => a.ResultID == 1, ct);
        var rejected = await _db.FinalDecisions.CountAsync(a => a.ResultID == 3, ct);
        var pending = await _db.FinalDecisions.CountAsync(a => a.ResultID ==2, ct);

        return new ApplicantsStatisticsDto
        {
            Total = total,
            Accepted = accepted,
            Rejected = rejected,
            Pending = pending
        };
    }
}
