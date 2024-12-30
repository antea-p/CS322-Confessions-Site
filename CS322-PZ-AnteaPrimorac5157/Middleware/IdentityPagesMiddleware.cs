namespace CS322_PZ_AnteaPrimorac5157.Middleware
{
    public class IdentityPagesMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<IdentityPagesMiddleware> _logger;

        public IdentityPagesMiddleware(RequestDelegate next, ILogger<IdentityPagesMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            // _logger.LogWarning($"Middleware intercepted path: {path}");

            if (path.StartsWithSegments("/Identity/Account", out var remaining))
            {
                var allowedPages = new[]
                {
                "/Login",
                "/Logout",
                "/ForgotPassword",
                "/AccessDenied"
            };

                if (!allowedPages.Any(allowed =>
                    remaining.Value.Equals(allowed, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning($"Redirecting from restricted path: {path}");
                    context.Response.Redirect("/Identity/Account/AccessDenied");
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension method for middleware
    public static class IdentityPagesMiddlewareExtensions
    {
        public static IApplicationBuilder UseIdentityPagesRestriction(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IdentityPagesMiddleware>();
        }
    }
}
