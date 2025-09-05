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
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Repositories
builder.Services.AddScoped(typeof(IPagedRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// FileNumber Generator
builder.Services.AddScoped<IFileNumberGenerator<Applicant>, ApplicantFileNumberGenerator>();

// DbContexts
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>()
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

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    await SeedRoles(roleManager);
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
    var roles = new[] { "Admin", "Supervisor", "Doctor", "Receptionist" , "Diwan" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
    }
}