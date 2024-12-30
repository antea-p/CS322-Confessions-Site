using Microsoft.AspNetCore.Authorization;

namespace CS322_PZ_AnteaPrimorac5157.Filters
{
    public class IdentityPagesAuthorizationHandler : AuthorizationHandler<IdentityPageRequirement>
    {
        private readonly ILogger<IdentityPagesAuthorizationHandler> _logger;

        public IdentityPagesAuthorizationHandler(ILogger<IdentityPagesAuthorizationHandler> logger)
        {
            _logger = logger;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IdentityPageRequirement requirement)
        {
            var httpContext = context.Resource as HttpContext;
            if (httpContext == null) return Task.CompletedTask;

            var path = httpContext.Request.Path;
            _logger.LogWarning($"Authorization check for path: {path}");

            var allowedPages = new[]
            {
            "/Identity/Account/Login",
            "/Identity/Account/Logout",
            "/Identity/Account/ForgotPassword",
            "/Identity/Account/AccessDenied"
        };

            if (path.StartsWithSegments("/Identity/Account", out var remaining) &&
                !allowedPages.Contains(path.Value, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning($"Blocking access to path: {path}");
                httpContext.Response.Redirect("/Identity/Account/AccessDenied");
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

public class IdentityPageRequirement : IAuthorizationRequirement { }