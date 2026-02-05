using Application.Abstractions;
using Application.DTOs.Users;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Models;
using Infrastructure.Services;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));



// Mapster Config & Mapper
var config = TypeAdapterConfig.GlobalSettings;
// Ensure FinalDecision mapping includes all properties
config.NewConfig<Infrastructure.Persistence.Models.FinalDecision, Application.DTOs.FinalDecisionDto>()
    .Map(dest => dest.ReceptionAddedAt, src => src.ReceptionAddedAt)
    .Map(dest => dest.SupervisorAddedAt, src => src.SupervisorAddedAt)
    .Map(dest => dest.SupervisorLastModifiedAt, src => src.SupervisorLastModifiedAt)
    .Map(dest => dest.IsExportedToRecruitment, src => src.IsExportedToRecruitment)
    .Map(dest => dest.ExportedAt, src => src.ExportedAt);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Repositories
builder.Services.AddScoped(typeof(IPagedRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// FileNumber Generator
builder.Services.AddScoped<IFileNumberGenerator<Applicant>, ApplicantFileNumberGenerator>();

// DbContexts
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        }));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // تخفيف متطلبات كلمة المرور للتطوير
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

// JWT
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    // تخصيص رسائل الأخطاء
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // لازم نوقف الـ default handler
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                succeeded = false,
                status = 401,
                message = "Unauthorized: Please login with a valid token",
                data = (object?)null,
                errors = new[] { "Invalid or expired token" },
                traceId = context.HttpContext.TraceIdentifier
            });

            return context.Response.WriteAsync(result);
        }
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SwaggerFileOperationFilter>();

    // إضافة تعريف الـ Bearer Token
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token.\r\nExample: \"Bearer eyJhbGciOi...\""
    });


    // تفعيل سياسة الأمان الافتراضية
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// MediatR Handlers
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining(typeof(GenericCommandHandler<,>));
    cfg.RegisterServicesFromAssemblyContaining(typeof(GenericQueryHandler<,>));
});

// Commands
// --- Register closed generic MediatR handlers dynamically for Entities <-> Request/Dto pairs ---
void RegisterGenericHandlers(IServiceCollection services)
{
    var allTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a =>
        {
            try { return a.GetTypes(); }
            catch { return Array.Empty<Type>(); }
        })
        .ToArray();

    // تعديل هذه الأسماء إن كانت مساحات الأسماء عندك مختلفة
    var entityTypes = allTypes
        .Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null &&
                    t.Namespace.Contains("Infrastructure.Persistence.Models"))
        .ToArray();

    var dtoTypes = allTypes
      .Where(t => t.IsClass && !t.IsAbstract &&
                  t.Namespace != null &&
                  t.Namespace.Contains("Application.DTOs")) 
      .ToArray();



    var requestTypes = allTypes
        .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Request"))
        .ToArray();

    foreach (var entity in entityTypes)
    {
        var baseName = entity.Name; // EyeExam

        var requestType = requestTypes.FirstOrDefault(t => t.Name.StartsWith(baseName, StringComparison.OrdinalIgnoreCase));
        var dtoType = dtoTypes.FirstOrDefault(t => t.Name.StartsWith(baseName, StringComparison.OrdinalIgnoreCase));


        if (requestType is null && dtoType is null) continue;

        // Register CreateEntityCommand<TEntity, TDto> -> TDto (or Request as response per your design)
        if (requestType != null)
        {
            var createReq = typeof(CreateEntityCommand<,>).MakeGenericType(entity, requestType);
            var updateReq = typeof(UpdateEntityCommand<,>).MakeGenericType(entity, requestType);

            var createInterface = typeof(IRequestHandler<,>).MakeGenericType(createReq, requestType);
            var updateInterface = typeof(IRequestHandler<,>).MakeGenericType(updateReq, requestType);

            var impl = typeof(GenericCommandHandler<,>).MakeGenericType(entity, requestType);

            services.AddTransient(createInterface, impl);
            services.AddTransient(updateInterface, impl);

            // Delete
            var deleteReq = typeof(DeleteEntityCommand<>).MakeGenericType(entity);
            var deleteInterface = typeof(IRequestHandler<,>).MakeGenericType(deleteReq, typeof(bool));
            services.AddTransient(deleteInterface, impl);
        }

        if (dtoType != null)
        {
            var getEntitiesReq = typeof(GetEntitiesQuery<,>).MakeGenericType(entity, dtoType);
            var pagedResultOfDto = typeof(PagedResult<>).MakeGenericType(dtoType);
            var getEntitiesInterface = typeof(IRequestHandler<,>).MakeGenericType(getEntitiesReq, pagedResultOfDto);

            var getByIdReq = typeof(GetEntityByIdQuery<,>).MakeGenericType(entity, dtoType);
            var getByIdInterface = typeof(IRequestHandler<,>).MakeGenericType(getByIdReq, dtoType);

            var impl = typeof(GenericQueryHandler<,>).MakeGenericType(entity, dtoType);

            services.AddTransient(getEntitiesInterface, impl);
            services.AddTransient(getByIdInterface, impl);
        }

        // Register Queries if dtoType exists
    
    }
}

RegisterGenericHandlers(builder.Services);
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDoctorQueryService, DoctorQueryService>();

builder.Services.AddScoped<IDoctorService, DoctorService>();

builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IArchiveService, ArchiveService>();
builder.Services.AddScoped<IRecruitmentExportService, RecruitmentExportService>();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // Don't ignore null values - show all fields
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.Never;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    await SeedRoles(roleManager);
    await SeedDoctors(dbContext);
    await SeedUsers(userManager);
}

// Middleware & Logging
app.UseSerilogRequestLogging();
builder.Services.AddLogging();
app.UseStaticFiles(); // هذا لــ wwwroot

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Files")),
    RequestPath = "/Files"
});
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "Files")),
    RequestPath = "/Files"
});
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<RoleAuthorizationMiddleware>();

app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var traceId = ctx.TraceIdentifier;
        var payload = ApiResult.Fail("Unhandled error", 500, new() { { "detail", new[] { ex.Message } } }, traceId);
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(payload);
    }
});

app.MapControllers();


app.Run();
static async Task SeedRoles(RoleManager<IdentityRole<int>> roleManager)
{
    var roles = new[] { "Admin", "Supervisor", "Doctor", "Receptionist", "Diwan" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
    }
}

static async Task SeedDoctors(AppDbContext dbContext)
{
    // Check if doctors already exist
    if (await dbContext.Doctors.AnyAsync())
        return;

    var doctors = new[]
    {
        new Doctor { DoctorID = 1, FullName = "د. أحمد خليل", SpecializationID = 1, ContractTypeID = 2, Code = "DOC001" },
        new Doctor { DoctorID = 2, FullName = "د. سامر محمود", SpecializationID = 2, ContractTypeID = 2, Code = "DOC002" },
        new Doctor { DoctorID = 3, FullName = "د. رامي حسن", SpecializationID = 3, ContractTypeID = 2, Code = "DOC003" },
        new Doctor { DoctorID = 4, FullName = "د. يوسف علي", SpecializationID = 4, ContractTypeID = 2, Code = "DOC004" },
        new Doctor { DoctorID = 5, FullName = "د. أحمد محمد", SpecializationID = 5, ContractTypeID = 2, Code = "DOC005" }
    };

    var strategy = dbContext.Database.CreateExecutionStrategy();
    await strategy.ExecuteAsync(doctors, async doctorsArray =>
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Doctors ON");
            foreach (var doctor in doctorsArray)
                dbContext.Doctors.Add(doctor);
            await dbContext.SaveChangesAsync();
            await dbContext.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Doctors OFF");
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    });
}

static async Task SeedUsers(UserManager<ApplicationUser> userManager)
{
    // Check if users already exist
    if (await userManager.Users.AnyAsync())
        return;

    var usersData = new[]
    {
        new { FullName = "موظف الاستقبال", UserName = "reception", Password = "1234", Role = "Receptionist", DoctorID = (int?)null },
        new { FullName = "مدير النظام", UserName = "admin", Password = "1234", Role = "Admin", DoctorID = (int?)null },
        new { FullName = "د. أحمد خليل", UserName = "eye_doc", Password = "1234", Role = "Doctor", DoctorID = (int?)1 },
        new { FullName = "د. سامر محمود", UserName = "internal_doc", Password = "1234", Role = "Doctor", DoctorID = (int?)2 },
        new { FullName = "د. رامي حسن", UserName = "surgery_doc", Password = "1234", Role = "Doctor", DoctorID = (int?)3 },
        new { FullName = "د. يوسف علي", UserName = "ortho_doc", Password = "1234", Role = "Doctor", DoctorID = (int?)4 },
        new { FullName = "الديوان", UserName = "diwan", Password = "1234", Role = "Diwan", DoctorID = (int?)null },
        new { FullName = "مشرف النظام", UserName = "supervisor", Password = "1234", Role = "Supervisor", DoctorID = (int?)null },
        new { FullName = "د. أحمد محمد", UserName = "ear_clinic", Password = "1234", Role = "Doctor", DoctorID = (int?)5 }
    };

    foreach (var userData in usersData)
    {
        var user = new ApplicationUser
        {
            FullName = userData.FullName,
            UserName = userData.UserName,
            Status = "Active"
        };

        if (userData.DoctorID.HasValue)
        {
            user.DoctorID = userData.DoctorID;
        }

        var result = await userManager.CreateAsync(user, userData.Password);
        
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, userData.Role);
        }
    }
}