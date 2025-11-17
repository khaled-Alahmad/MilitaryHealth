using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AspNetRo__3214EC07E0371F6E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EntityName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    EntityKey = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ChangedColumns = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    OldValues = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    NewValues = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__A17F2398AB6847B7", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "ContractTypes",
                columns: table => new
                {
                    ContractTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Contract__68A6154545ADAD3C", x => x.ContractTypeID);
                });

            migrationBuilder.CreateTable(
                name: "MaritalStatus",
                columns: table => new
                {
                    MaritalStatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MaritalS__C8B1BA52E679DDF6", x => x.MaritalStatusID);
                });

            migrationBuilder.CreateTable(
                name: "RefractionTypes",
                columns: table => new
                {
                    RefractionTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Refracti__75753A9CCE19D300", x => x.RefractionTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ResultID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Results__9769022815022F28", x => x.ResultID);
                });

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    SpecializationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Speciali__5809D84F67128FD1", x => x.SpecializationID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AspNetRo__3214EC07CA93C2E6", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Applicants",
                columns: table => new
                {
                    ApplicantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    AssociateNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "غير محدد"),
                    FullName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    MotherName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime", nullable: true),
                    RecruitmentCenter = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    BloodType = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    MaritalStatusID = table.Column<int>(type: "int", nullable: true),
                    Job = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Height = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BMI = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BloodPressure = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Pulse = table.Column<int>(type: "int", nullable: true),
                    Tattoo = table.Column<bool>(type: "bit", nullable: true),
                    DistinctiveMarks = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Applican__39AE9148E8810501", x => x.ApplicantID);
                    table.UniqueConstraint("AK_Applicants_FileNumber", x => x.FileNumber);
                    table.ForeignKey(
                        name: "FK__Applicant__Marit__45F365D3",
                        column: x => x.MaritalStatusID,
                        principalTable: "MaritalStatus",
                        principalColumn: "MaritalStatusID");
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    DoctorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    SpecializationID = table.Column<int>(type: "int", nullable: true),
                    ContractTypeID = table.Column<int>(type: "int", nullable: true),
                    Code = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Doctors__2DC00EDF0F91A5A9", x => x.DoctorID);
                    table.ForeignKey(
                        name: "FK__Doctors__Contrac__3E52440B",
                        column: x => x.ContractTypeID,
                        principalTable: "ContractTypes",
                        principalColumn: "ContractTypeID");
                    table.ForeignKey(
                        name: "FK__Doctors__Special__3D5E1FD2",
                        column: x => x.SpecializationID,
                        principalTable: "Specializations",
                        principalColumn: "SpecializationID");
                });

            migrationBuilder.CreateTable(
                name: "Archive",
                columns: table => new
                {
                    ArchiveID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantID = table.Column<int>(type: "int", nullable: false),
                    DecisionID = table.Column<int>(type: "int", nullable: false),
                    FileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    DigitalCopy = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archive_1", x => new { x.ArchiveID, x.ApplicantID, x.DecisionID });
                    table.ForeignKey(
                        name: "FK_Archive_Applicants",
                        column: x => x.ApplicantFileNumber,
                        principalTable: "Applicants",
                        principalColumn: "FileNumber");
                    table.ForeignKey(
                        name: "FK__Archive__Applica__6A30C649",
                        column: x => x.ApplicantID,
                        principalTable: "Applicants",
                        principalColumn: "ApplicantID");
                });

            migrationBuilder.CreateTable(
                name: "Consultations",
                columns: table => new
                {
                    ConsultationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ConsultationType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ReferredDoctor = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Result = table.Column<string>(type: "text", nullable: true),
                    Attachment = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Consulta__5D014A78B8B974D2", x => x.ConsultationID);
                    table.ForeignKey(
                        name: "FK_Consultations_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                });

            migrationBuilder.CreateTable(
                name: "EarClinicExam",
                columns: table => new
                {
                    EarClinicID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    RightEar = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeftEar = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RightTympanicMembrane = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeftTympanicMembrane = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RightHearing = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeftHearing = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Resonators = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RightWhisperTest = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeftWhisperTest = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RightNose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeftNose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    isRightHugeMates = table.Column<bool>(type: "bit", nullable: true),
                    isLeftHugeMates = table.Column<bool>(type: "bit", nullable: true),
                    RightString = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LeftString = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Mouth = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OtherDiseases = table.Column<string>(type: "text", nullable: true),
                    ResultID = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarClinicExam", x => x.EarClinicID);
                    table.ForeignKey(
                        name: "FK_EarClinicExam_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "FK_EarClinicExam_Results",
                        column: x => x.ResultID,
                        principalTable: "Results",
                        principalColumn: "ResultID");
                });

            migrationBuilder.CreateTable(
                name: "EyeExam",
                columns: table => new
                {
                    EyeExamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    Vision = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    VisionLeft = table.Column<string>(type: "nchar(10)", fixedLength: true, maxLength: 10, nullable: true),
                    ColorTestLeft = table.Column<string>(type: "nchar(10)", fixedLength: true, maxLength: 10, nullable: true),
                    ColorTest = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    OtherDiseases = table.Column<string>(type: "text", nullable: true),
                    ResultID = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__EyeExam__C99F26ADECA9F5D7", x => x.EyeExamID);
                    table.ForeignKey(
                        name: "FK_EyeExam_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "FK__EyeExam__ResultI__4F7CD00D",
                        column: x => x.ResultID,
                        principalTable: "Results",
                        principalColumn: "ResultID");
                });

            migrationBuilder.CreateTable(
                name: "InternalExam",
                columns: table => new
                {
                    InternalExamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    Heart = table.Column<string>(type: "text", nullable: true),
                    Respiratory = table.Column<string>(type: "text", nullable: true),
                    Digestive = table.Column<string>(type: "text", nullable: true),
                    Endocrine = table.Column<string>(type: "text", nullable: true),
                    Neurology = table.Column<string>(type: "text", nullable: true),
                    Blood = table.Column<string>(type: "text", nullable: true),
                    Joints = table.Column<string>(type: "text", nullable: true),
                    Kidney = table.Column<string>(type: "text", nullable: true),
                    Skin = table.Column<string>(type: "text", nullable: true),
                    ResultID = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Internal__E33A763CB54CBD2F", x => x.InternalExamID);
                    table.ForeignKey(
                        name: "FK_InternalExam_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "FK__InternalE__Resul__5441852A",
                        column: x => x.ResultID,
                        principalTable: "Results",
                        principalColumn: "ResultID");
                });

            migrationBuilder.CreateTable(
                name: "Investigations",
                columns: table => new
                {
                    InvestigationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Result = table.Column<string>(type: "text", nullable: true),
                    Attachment = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Investig__83714CD4B4BFB7A1", x => x.InvestigationID);
                    table.ForeignKey(
                        name: "FK_Investigations_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                });

            migrationBuilder.CreateTable(
                name: "OrthopedicExam",
                columns: table => new
                {
                    OrthopedicExamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    Musculoskeletal = table.Column<string>(type: "text", nullable: true),
                    NeurologicalSurgery = table.Column<string>(type: "text", nullable: true),
                    ResultID = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Orthoped__FB01E23AF75139AD", x => x.OrthopedicExamID);
                    table.ForeignKey(
                        name: "FK_OrthopedicExam_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "FK__Orthopedi__Resul__5DCAEF64",
                        column: x => x.ResultID,
                        principalTable: "Results",
                        principalColumn: "ResultID");
                });

            migrationBuilder.CreateTable(
                name: "SurgicalExam",
                columns: table => new
                {
                    SurgicalExamID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    GeneralSurgery = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    UrinarySurgery = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    VascularSurgery = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ThoracicSurgery = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ResultID = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Surgical__847BC15AD83A82FE", x => x.SurgicalExamID);
                    table.ForeignKey(
                        name: "FK_SurgicalExam_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                    table.ForeignKey(
                        name: "FK__SurgicalE__Resul__59063A47",
                        column: x => x.ResultID,
                        principalTable: "Results",
                        principalColumn: "ResultID");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DoctorID = table.Column<int>(type: "int", nullable: true),
                    Permissions = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    LastLogin = table.Column<DateTime>(type: "datetime", nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__1788CCAC2F38B562", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Doctors",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                });

            migrationBuilder.CreateTable(
                name: "Refractions",
                columns: table => new
                {
                    RefractionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefractionTypeID = table.Column<int>(type: "int", nullable: false),
                    RefractionValue = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsLeft = table.Column<bool>(type: "bit", nullable: true),
                    EyeExamID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refractions", x => new { x.RefractionID, x.RefractionTypeID });
                    table.ForeignKey(
                        name: "FK_Refractions_EyeExam",
                        column: x => x.EyeExamID,
                        principalTable: "EyeExam",
                        principalColumn: "EyeExamID");
                    table.ForeignKey(
                        name: "FK_Refractions_RefractionTypes",
                        column: x => x.RefractionTypeID,
                        principalTable: "RefractionTypes",
                        principalColumn: "RefractionTypeID");
                });

            migrationBuilder.CreateTable(
                name: "FinalDecision",
                columns: table => new
                {
                    DecisionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrthopedicExamID = table.Column<int>(type: "int", nullable: false),
                    SurgicalExamID = table.Column<int>(type: "int", nullable: false),
                    InternalExamID = table.Column<int>(type: "int", nullable: false),
                    EyeExamID = table.Column<int>(type: "int", nullable: false),
                    EarClinicID = table.Column<int>(type: "int", nullable: false),
                    ApplicantFileNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ResultID = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    PostponeDuration = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    DecisionDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinalDecision", x => new { x.DecisionID, x.OrthopedicExamID, x.SurgicalExamID, x.InternalExamID, x.EyeExamID, x.EarClinicID });
                    table.ForeignKey(
                        name: "FK_FinalDecision_EarClinicExam",
                        column: x => x.EarClinicID,
                        principalTable: "EarClinicExam",
                        principalColumn: "EarClinicID");
                    table.ForeignKey(
                        name: "FK_FinalDecision_EyeExam",
                        column: x => x.EyeExamID,
                        principalTable: "EyeExam",
                        principalColumn: "EyeExamID");
                    table.ForeignKey(
                        name: "FK_FinalDecision_InternalExam",
                        column: x => x.InternalExamID,
                        principalTable: "InternalExam",
                        principalColumn: "InternalExamID");
                    table.ForeignKey(
                        name: "FK_FinalDecision_OrthopedicExam",
                        column: x => x.OrthopedicExamID,
                        principalTable: "OrthopedicExam",
                        principalColumn: "OrthopedicExamID");
                    table.ForeignKey(
                        name: "FK_FinalDecision_SurgicalExam",
                        column: x => x.SurgicalExamID,
                        principalTable: "SurgicalExam",
                        principalColumn: "SurgicalExamID");
                    table.ForeignKey(
                        name: "FK__FinalDeci__Resul__6754599E",
                        column: x => x.ResultID,
                        principalTable: "Results",
                        principalColumn: "ResultID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AspNetUs__3214EC07EF4F5060", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Roles",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "UserRefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    JwtId = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    ExpiresOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    RevokedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserRefr__3214EC074F2E5198", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRefreshTokens_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applicants_MaritalStatusID",
                table: "Applicants",
                column: "MaritalStatusID");

            migrationBuilder.CreateIndex(
                name: "UQ__Applican__8BD00B71D756A649",
                table: "Applicants",
                column: "FileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Archive_ApplicantFileNumber",
                table: "Archive",
                column: "ApplicantFileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Archive_ApplicantID",
                table: "Archive",
                column: "ApplicantID");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_NormalizedName",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "([NormalizedName] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_DoctorID",
                table: "Consultations",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors",
                table: "Doctors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_ContractTypeID",
                table: "Doctors",
                column: "ContractTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_SpecializationID",
                table: "Doctors",
                column: "SpecializationID");

            migrationBuilder.CreateIndex(
                name: "IX_EarClinicExam_DoctorID",
                table: "EarClinicExam",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_EarClinicExam_ResultID",
                table: "EarClinicExam",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_EyeExam",
                table: "EyeExam",
                column: "ApplicantFileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EyeExam_DoctorID",
                table: "EyeExam",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_EyeExam_ResultID",
                table: "EyeExam",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_FinalDecision",
                table: "FinalDecision",
                column: "ApplicantFileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_FinalDecision_EarClinicID",
                table: "FinalDecision",
                column: "EarClinicID");

            migrationBuilder.CreateIndex(
                name: "IX_FinalDecision_EyeExamID",
                table: "FinalDecision",
                column: "EyeExamID");

            migrationBuilder.CreateIndex(
                name: "IX_FinalDecision_InternalExamID",
                table: "FinalDecision",
                column: "InternalExamID");

            migrationBuilder.CreateIndex(
                name: "IX_FinalDecision_OrthopedicExamID",
                table: "FinalDecision",
                column: "OrthopedicExamID");

            migrationBuilder.CreateIndex(
                name: "IX_FinalDecision_ResultID",
                table: "FinalDecision",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_FinalDecision_SurgicalExamID",
                table: "FinalDecision",
                column: "SurgicalExamID");

            migrationBuilder.CreateIndex(
                name: "IX_InternalExam",
                table: "InternalExam",
                column: "ApplicantFileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InternalExam_DoctorID",
                table: "InternalExam",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_InternalExam_ResultID",
                table: "InternalExam",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_DoctorID",
                table: "Investigations",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_OrthopedicExam",
                table: "OrthopedicExam",
                column: "ApplicantFileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrthopedicExam_DoctorID",
                table: "OrthopedicExam",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_OrthopedicExam_ResultID",
                table: "OrthopedicExam",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_Refractions_EyeExamID",
                table: "Refractions",
                column: "EyeExamID");

            migrationBuilder.CreateIndex(
                name: "IX_Refractions_RefractionTypeID",
                table: "Refractions",
                column: "RefractionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_SurgicalExam",
                table: "SurgicalExam",
                column: "ApplicantFileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SurgicalExam_DoctorID",
                table: "SurgicalExam",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_SurgicalExam_ResultID",
                table: "SurgicalExam",
                column: "ResultID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_Token",
                table: "UserRefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRefreshTokens_UserId",
                table: "UserRefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DoctorID",
                table: "Users",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedEmail",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NormalizedUserName",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "([NormalizedUserName] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E47EFD7CAF",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archive");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Consultations");

            migrationBuilder.DropTable(
                name: "FinalDecision");

            migrationBuilder.DropTable(
                name: "Investigations");

            migrationBuilder.DropTable(
                name: "Refractions");

            migrationBuilder.DropTable(
                name: "UserRefreshTokens");

            migrationBuilder.DropTable(
                name: "Applicants");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "EarClinicExam");

            migrationBuilder.DropTable(
                name: "InternalExam");

            migrationBuilder.DropTable(
                name: "OrthopedicExam");

            migrationBuilder.DropTable(
                name: "SurgicalExam");

            migrationBuilder.DropTable(
                name: "EyeExam");

            migrationBuilder.DropTable(
                name: "RefractionTypes");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MaritalStatus");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "ContractTypes");

            migrationBuilder.DropTable(
                name: "Specializations");
        }
    }
}
