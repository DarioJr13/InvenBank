namespace InvenBank.API.Middleware
{
    public class CorsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorsMiddleware> _logger;

        public CorsMiddleware(RequestDelegate next, ILogger<CorsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var origin = context.Request.Headers["Origin"].ToString();

            if (!string.IsNullOrEmpty(origin))
            {
                context.Response.Headers.Add("Access-Control-Allow-Origin", origin);
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            if (context.Request.Method == "OPTIONS")
            {
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type,Authorization,X-Requested-With");
                context.Response.StatusCode = 200;
                return;
            }

            await _next(context);
        }
    }
}
