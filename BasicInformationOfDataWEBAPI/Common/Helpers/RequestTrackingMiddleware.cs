namespace BasicInformationOfDataWEBAPI.Common.Helpers
{
    public class RequestTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestTracker _tracker;

        public RequestTrackingMiddleware(RequestDelegate next, RequestTracker tracker)
        {
            _next = next;
            _tracker = tracker;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // 允许 enable 和 health 在禁用时访问
            if (!_tracker.IsEnabled &&
                path != "/enable"
               && path != "/health"
                )
            {
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Instance disabled");
                return;
            }

            _tracker.Increment();
            try
            {
                await _next(context);
            }
            finally
            {
                _tracker.Decrement();
            }
        }
    }
}
