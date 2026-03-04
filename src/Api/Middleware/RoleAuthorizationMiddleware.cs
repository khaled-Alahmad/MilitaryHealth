using Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

public class RoleAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleAuthorizationMiddleware> _logger;

    public RoleAuthorizationMiddleware(RequestDelegate next, ILogger<RoleAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint == null)
        {
            // طلبات الملفات الثابتة /Files/ قد تصل هنا إذا الملف غير موجود — نمرّرها ليعيد الإطار 404 عادي
            if (context.Request.Path.StartsWithSegments("/Files", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteJsonResponse(context, 404, "Endpoint not found");
            return;
        }

        // السماح للـ Anonymous
        if (endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }

        // التحقق من المصادقة
        if (context.User.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await WriteJsonResponse(context, 401, "Unauthorized: Please login");
            return;
        }

        // التحقق من الصلاحيات
        var authAttrs = endpoint.Metadata.GetOrderedMetadata<AuthorizeAttribute>();
        var requiredRoles = authAttrs
            .Where(a => !string.IsNullOrWhiteSpace(a.Roles))
            .SelectMany(a => a.Roles.Split(','))
            .Select(r => r.Trim())
            .Where(r => !string.IsNullOrEmpty(r))
            .Distinct()
            .ToList();

        if (requiredRoles.Any() && !requiredRoles.Any(r => context.User.IsInRole(r)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await WriteJsonResponse(context, 403, "Forbidden: You do not have permission");
            return;
        }

        await _next(context);
    }

    private static Task WriteJsonResponse(HttpContext context, int status, string message)
    {
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            succeeded = false,
            status,
            message,
            data = (object?)null,
            errors = new[] { message },
            traceId = context.TraceIdentifier
        });

        return context.Response.WriteAsync(result);
    }
}
