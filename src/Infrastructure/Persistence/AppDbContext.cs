using System;
using System.Collections.Generic;
using Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Applicant> Applicants { get; set; }

    public virtual DbSet<Archive> Archives { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Consultation> Consultations { get; set; }

    public virtual DbSet<ContractType> ContractTypes { get; set; }

    public virtual DbSet<Doctor> Doctors { get; set; }

    public virtual DbSet<EarClinicExam> EarClinicExams { get; set; }

    public virtual DbSet<EyeExam> EyeExams { get; set; }

    public virtual DbSet<FinalDecision> FinalDecisions { get; set; }

    public virtual DbSet<InternalExam> InternalExams { get; set; }

    public virtual DbSet<Investigation> Investigations { get; set; }

    public virtual DbSet<MaritalStatus> MaritalStatuses { get; set; }

    public virtual DbSet<OrthopedicExam> OrthopedicExams { get; set; }

    public virtual DbSet<Refraction> Refractions { get; set; }

    public virtual DbSet<RefractionType> RefractionTypes { get; set; }

    public virtual DbSet<Result> Results { get; set; }

    public virtual DbSet<Specialization> Specializations { get; set; }

    public virtual DbSet<SurgicalExam> SurgicalExams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_CI_AS");

        modelBuilder.Entity<Applicant>(entity =>
        {
            entity.HasKey(e => e.ApplicantID).HasName("PK__Applican__39AE9148E8810501");

            entity.HasIndex(e => e.FileNumber, "UQ__Applican__8BD00B71D756A649").IsUnique();
            
            // Configure trigger to avoid OUTPUT clause issues
            entity.ToTable(tb => tb.HasTrigger("trg_GenerateQueueNumber"));

            entity.Property(e => e.AssociateNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("غير محدد");
            entity.Property(e => e.BMI).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.BloodPressure)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.DistinctiveMarks).HasColumnType("text");
            entity.Property(e => e.FileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MotherName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.DateOfBirth).HasColumnType("datetime");
            entity.Property(e => e.RecruitmentCenter)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.BloodType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Height).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Job)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.QueueNumber);

            entity.HasOne(d => d.MaritalStatus).WithMany(p => p.Applicants)
                .HasForeignKey(d => d.MaritalStatusID)
                .HasConstraintName("FK__Applicant__Marit__45F365D3");
        });

        modelBuilder.Entity<Archive>(entity =>
        {
            entity.HasKey(e => new { e.ArchiveID, e.ApplicantID, e.DecisionID }).HasName("PK_Archive_1");

            entity.ToTable("Archive");

            entity.Property(e => e.ArchiveID).ValueGeneratedOnAdd();
            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ArchiveDate).HasColumnType("datetime");
            entity.Property(e => e.DigitalCopy).IsUnicode(false);
            entity.Property(e => e.FileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ApplicantFileNumberNavigation).WithMany(p => p.ArchiveApplicantFileNumberNavigations)
                .HasPrincipalKey(p => p.FileNumber)
                .HasForeignKey(d => d.ApplicantFileNumber)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Archive_Applicants");

            entity.HasOne(d => d.Applicant).WithMany(p => p.ArchiveApplicants)
                .HasForeignKey(d => d.ApplicantID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Archive__Applica__6A30C649");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetRo__3214EC07E0371F6E");

            entity.HasIndex(e => e.NormalizedName, "IX_AspNetRoles_NormalizedName")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.ConcurrencyStamp).HasMaxLength(256);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetRo__3214EC07CA93C2E6");

            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AspNetRoleClaims_Roles");
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AspNetUs__3214EC07EF4F5060");

            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AspNetUserClaims_Users");
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);
            entity.Property(e => e.ProviderDisplayName).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AspNetUserLogins_Users");
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AspNetUserTokens_Users");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__AuditLog__A17F2398AB6847B7");

            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ChangedColumns).IsUnicode(false);
            entity.Property(e => e.EntityKey)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.EntityName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NewValues).IsUnicode(false);
            entity.Property(e => e.OldValues).IsUnicode(false);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.ConsultationID).HasName("PK__Consulta__5D014A78B8B974D2");

            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Attachment)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ConsultationType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ReferralReason)
                .HasColumnType("text");
            entity.Property(e => e.Result).HasColumnType("text");

            entity.HasOne(d => d.Doctor).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.DoctorID)
                .HasConstraintName("FK_Consultations_Doctors");
        });

        modelBuilder.Entity<ContractType>(entity =>
        {
            entity.HasKey(e => e.ContractTypeID).HasName("PK__Contract__68A6154545ADAD3C");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.HasKey(e => e.DoctorID).HasName("PK__Doctors__2DC00EDF0F91A5A9");

            entity.HasIndex(e => e.Code, "IX_Doctors").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.ContractType).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.ContractTypeID)
                .HasConstraintName("FK__Doctors__Contrac__3E52440B");

            entity.HasOne(d => d.Specialization).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.SpecializationID)
                .HasConstraintName("FK__Doctors__Special__3D5E1FD2");
        });

        modelBuilder.Entity<EarClinicExam>(entity =>
        {
            entity.HasKey(e => e.EarClinicID);

            entity.ToTable("EarClinicExam");

            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LeftEar).HasMaxLength(50);
            entity.Property(e => e.LeftHearing).HasMaxLength(50);
            entity.Property(e => e.LeftNose).HasMaxLength(50);
            entity.Property(e => e.LeftString).HasMaxLength(50);
            entity.Property(e => e.LeftTympanicMembrane).HasMaxLength(50);
            entity.Property(e => e.LeftWhisperTest).HasMaxLength(50);
            entity.Property(e => e.Mouth).HasMaxLength(50);
            entity.Property(e => e.OtherDiseases).HasColumnType("text");
            entity.Property(e => e.Reason).HasColumnType("text");
            entity.Property(e => e.Resonators).HasMaxLength(50);
            entity.Property(e => e.RightEar).HasMaxLength(50);
            entity.Property(e => e.RightHearing).HasMaxLength(50);
            entity.Property(e => e.RightNose).HasMaxLength(50);
            entity.Property(e => e.RightString).HasMaxLength(50);
            entity.Property(e => e.RightTympanicMembrane).HasMaxLength(50);
            entity.Property(e => e.RightWhisperTest).HasMaxLength(50);

            entity.HasOne(d => d.Doctor).WithMany(p => p.EarClinicExams)
                .HasForeignKey(d => d.DoctorID)
                .HasConstraintName("FK_EarClinicExam_Doctors");

            entity.HasOne(d => d.Result).WithMany(p => p.EarClinicExams)
                .HasForeignKey(d => d.ResultID)
                .HasConstraintName("FK_EarClinicExam_Results");
        });

        modelBuilder.Entity<EyeExam>(entity =>
        {
            entity.HasKey(e => e.EyeExamID).HasName("PK__EyeExam__C99F26ADECA9F5D7");

            entity.ToTable("EyeExam");

            entity.HasIndex(e => e.ApplicantFileNumber, "IX_EyeExam").IsUnique();

            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ColorTest)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ColorTestLeft)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Vision)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VisionLeft)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.WorstRefractionRight)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.WorstRefractionLeft)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OtherDiseases).HasColumnType("text");
            entity.Property(e => e.Reason).HasColumnType("text");

            entity.HasOne(d => d.Doctor).WithMany(p => p.EyeExams)
                .HasForeignKey(d => d.DoctorID)
                .HasConstraintName("FK_EyeExam_Doctors");

            entity.HasOne(d => d.Result).WithMany(p => p.EyeExams)
                .HasForeignKey(d => d.ResultID)
                .HasConstraintName("FK__EyeExam__ResultI__4F7CD00D");
        });

        modelBuilder.Entity<FinalDecision>(entity =>
        {
            entity.HasKey(e => new { e.DecisionID, e.OrthopedicExamID, e.SurgicalExamID, e.InternalExamID, e.EyeExamID, e.EarClinicID });

            entity.ToTable("FinalDecision");

            entity.HasIndex(e => e.ApplicantFileNumber, "IX_FinalDecision");

            entity.Property(e => e.DecisionID).ValueGeneratedOnAdd();
            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PostponeDuration)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Reason).HasColumnType("text");
            entity.Property(e => e.ReceptionAddedAt).HasColumnType("datetime");
            entity.Property(e => e.SupervisorAddedAt).HasColumnType("datetime");
            entity.Property(e => e.SupervisorLastModifiedAt).HasColumnType("datetime");
            entity.Property(e => e.IsExportedToRecruitment).HasDefaultValue(false);
            entity.Property(e => e.ExportedAt).HasColumnType("datetime");

            entity.HasOne(d => d.EarClinic).WithMany(p => p.FinalDecisions)
                .HasForeignKey(d => d.EarClinicID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FinalDecision_EarClinicExam");

            entity.HasOne(d => d.EyeExam).WithMany(p => p.FinalDecisions)
                .HasForeignKey(d => d.EyeExamID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FinalDecision_EyeExam");

            entity.HasOne(d => d.InternalExam).WithMany(p => p.FinalDecisions)
                .HasForeignKey(d => d.InternalExamID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FinalDecision_InternalExam");

            entity.HasOne(d => d.OrthopedicExam).WithMany(p => p.FinalDecisions)
                .HasForeignKey(d => d.OrthopedicExamID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FinalDecision_OrthopedicExam");

            entity.HasOne(d => d.Result).WithMany(p => p.FinalDecisions)
                .HasForeignKey(d => d.ResultID)
                .HasConstraintName("FK__FinalDeci__Resul__6754599E");

            entity.HasOne(d => d.SurgicalExam).WithMany(p => p.FinalDecisions)
                .HasForeignKey(d => d.SurgicalExamID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FinalDecision_SurgicalExam");
        });

        modelBuilder.Entity<InternalExam>(entity =>
        {
            entity.HasKey(e => e.InternalExamID).HasName("PK__Internal__E33A763CB54CBD2F");

            entity.ToTable("InternalExam");

            entity.HasIndex(e => e.ApplicantFileNumber, "IX_InternalExam").IsUnique();

            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Blood).HasColumnType("text");
            entity.Property(e => e.Digestive).HasColumnType("text");
            entity.Property(e => e.Endocrine).HasColumnType("text");
            entity.Property(e => e.Heart).HasColumnType("text");
            entity.Property(e => e.Joints).HasColumnType("text");
            entity.Property(e => e.Kidney).HasColumnType("text");
            entity.Property(e => e.Neurology).HasColumnType("text");
            entity.Property(e => e.Reason).HasColumnType("text");
            entity.Property(e => e.Respiratory).HasColumnType("text");
            entity.Property(e => e.Skin).HasColumnType("text");

            entity.HasOne(d => d.Doctor).WithMany(p => p.InternalExams)
                .HasForeignKey(d => d.DoctorID)
                .HasConstraintName("FK_InternalExam_Doctors");

            entity.HasOne(d => d.Result).WithMany(p => p.InternalExams)
                .HasForeignKey(d => d.ResultID)
                .HasConstraintName("FK__InternalE__Resul__5441852A");
        });

        modelBuilder.Entity<Investigation>(entity =>
        {
            entity.HasKey(e => e.InvestigationID).HasName("PK__Investig__83714CD4B4BFB7A1");

            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Attachment)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.InvestigationReason).HasColumnType("text");
            entity.Property(e => e.Result).HasColumnType("text");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Investigations)
                .HasForeignKey(d => d.DoctorID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Investigations_Doctors");
        });

        modelBuilder.Entity<MaritalStatus>(entity =>
        {
            entity.HasKey(e => e.MaritalStatusID).HasName("PK__MaritalS__C8B1BA52E679DDF6");

            entity.ToTable("MaritalStatus");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
         
        });

        modelBuilder.Entity<OrthopedicExam>(entity =>
        {
            entity.HasKey(e => e.OrthopedicExamID).HasName("PK__Orthoped__FB01E23AF75139AD");

            entity.ToTable("OrthopedicExam");

            entity.HasIndex(e => e.ApplicantFileNumber, "IX_OrthopedicExam").IsUnique();

            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Musculoskeletal).HasColumnType("text");
            entity.Property(e => e.NeurologicalSurgery).HasColumnType("text");
            entity.Property(e => e.Reason).HasColumnType("text");

            entity.HasOne(d => d.Doctor).WithMany(p => p.OrthopedicExams)
                .HasForeignKey(d => d.DoctorID)
                .HasConstraintName("FK_OrthopedicExam_Doctors");

            entity.HasOne(d => d.Result).WithMany(p => p.OrthopedicExams)
                .HasForeignKey(d => d.ResultID)
                .HasConstraintName("FK__Orthopedi__Resul__5DCAEF64");
        });

        modelBuilder.Entity<Refraction>(entity =>
        {
            entity.HasKey(e => new { e.RefractionID, e.RefractionTypeID });

            entity.Property(e => e.RefractionID).ValueGeneratedOnAdd();
            entity.Property(e => e.RefractionValue).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.EyeExam).WithMany(p => p.Refractions)
                .HasForeignKey(d => d.EyeExamID)
                .HasConstraintName("FK_Refractions_EyeExam");

            entity.HasOne(d => d.RefractionType).WithMany(p => p.Refractions)
                .HasForeignKey(d => d.RefractionTypeID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Refractions_RefractionTypes");
        });

        modelBuilder.Entity<RefractionType>(entity =>
        {
            entity.HasKey(e => e.RefractionTypeID).HasName("PK__Refracti__75753A9CCE19D300");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasKey(e => e.ResultID).HasName("PK__Results__9769022815022F28");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(e => e.SpecializationID).HasName("PK__Speciali__5809D84F67128FD1");

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SurgicalExam>(entity =>
        {
            entity.HasKey(e => e.SurgicalExamID).HasName("PK__Surgical__847BC15AD83A82FE");

            entity.ToTable("SurgicalExam");

            entity.HasIndex(e => e.ApplicantFileNumber, "IX_SurgicalExam");

            entity.Property(e => e.ApplicantFileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GeneralSurgery)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Reason).HasColumnType("text");
            entity.Property(e => e.ThoracicSurgery)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UrinarySurgery)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VascularSurgery)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Doctor).WithMany(p => p.SurgicalExams)
                .HasForeignKey(d => d.DoctorID)
                .HasConstraintName("FK_SurgicalExam_Doctors");

            entity.HasOne(d => d.Result).WithMany(p => p.SurgicalExams)
                .HasForeignKey(d => d.ResultID)
                .HasConstraintName("FK__SurgicalE__Resul__59063A47");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserID).HasName("PK__Users__1788CCAC2F38B562");

            entity.HasIndex(e => e.NormalizedEmail, "IX_Users_NormalizedEmail");

            entity.HasIndex(e => e.NormalizedUserName, "IX_Users_NormalizedUserName")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E47EFD7CAF").IsUnique();

            entity.Property(e => e.ConcurrencyStamp)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.LockoutEnabled).HasDefaultValue(true);
            entity.Property(e => e.NormalizedEmail)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.NormalizedUserName)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Permissions)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RefreshTokenExpiry).HasColumnType("datetime");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SecurityStamp)
                .HasMaxLength(256)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Doctor).WithMany(p => p.Users)
                .HasForeignKey(d => d.DoctorID)
                .HasConstraintName("FK_Users_Doctors");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AspNetUserRoles_Roles"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AspNetUserRoles_Users"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                        j.HasIndex(new[] { "UserId" }, "IX_AspNetUserRoles_UserId");
                    });
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRefr__3214EC074F2E5198");

            entity.HasIndex(e => e.Token, "IX_UserRefreshTokens_Token").IsUnique();

            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.JwtId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Token)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.UserRefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRefreshTokens_Users");
        });

        modelBuilder.Entity<Result>().HasData(
            new Result { ResultID = 1, Description = "مقبول" },
            new Result { ResultID = 2, Description = "مرفوض" },
            new Result { ResultID = 3, Description = "مؤجل" }
        );

        modelBuilder.Entity<Specialization>().HasData(
            new Specialization { SpecializationID = 1, Description = "عيون" },
            new Specialization { SpecializationID = 2, Description = "باطنة" },
            new Specialization { SpecializationID = 3, Description = "جراحة" },
            new Specialization { SpecializationID = 4, Description = "عظمية" },
            new Specialization { SpecializationID = 5, Description = "اذنية" }
        );

        modelBuilder.Entity<RefractionType>().HasData(
            new RefractionType { RefractionTypeID = 1, Description = "حسر نظر" },
            new RefractionType { RefractionTypeID = 2, Description = "مد نظر" },
            new RefractionType { RefractionTypeID = 3, Description = "استجماتيزم" },
            new RefractionType { RefractionTypeID = 4, Description = "سليم" }
        );

        modelBuilder.Entity<MaritalStatus>().HasData(
            new MaritalStatus { MaritalStatusID = 1, Description = "أعزب" },
            new MaritalStatus { MaritalStatusID = 2, Description = "متزوج" },
            new MaritalStatus { MaritalStatusID = 3, Description = "مطلق" }
        );

        modelBuilder.Entity<ContractType>().HasData(
            new ContractType { ContractTypeID = 1, Description = "مدني" },
            new ContractType { ContractTypeID = 2, Description = "عسكري" },
            new ContractType { ContractTypeID = 3, Description = "متعاقد" }
        );

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
