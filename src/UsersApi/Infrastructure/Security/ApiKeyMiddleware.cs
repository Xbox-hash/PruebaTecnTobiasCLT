using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace UsersApi.Infrastructure.Security
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _config;
        private const string ApiKeyHeaderName = "X-API-KEY";

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext context)
        {
            //aca vamos a permitir el acceso publico a swagger para poder ver la documentacion
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/swagger") || path.StartsWith("/favicon"))
            {
                await _next(context);
                return;
            }

            //tomamos la clave del appsettings
            var expectedApiKey = _config["ApiKey:Value"];

            //validamos si el codigo del apikey coincide


            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey) ||
          extractedApiKey != expectedApiKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "API key invalida o ausente"
                });
                return;
            }
            //si la clave esta correcta continua en el siguiente middleware o endpoint
            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}
