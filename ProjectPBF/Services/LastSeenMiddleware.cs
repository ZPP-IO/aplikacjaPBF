using Microsoft.AspNetCore.Identity;
using ProjectPBF.Models;
using System.Security.Claims;

namespace ProjectPBF.Services
{
    /// <summary>
    /// Middleware aktualizujący LastSeenAt zalogowanego użytkownika przy każdym żądaniu HTTP.
    /// Zapis do bazy następuje co 2 minuty, żeby nie uderzać w bazę przy każdym kliknięciu.
    /// </summary>
    public class LastSeenMiddleware
    {
        private readonly RequestDelegate _next;

        public LastSeenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<UserModel> userManager)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out var userId))
                {
                    // Klucz sesji przechowuje czas ostatniego zapisu do bazy,
                    // dzięki czemu nie robimy UPDATE przy każdym żądaniu.
                    var sessionKey = $"LastSeenSaved_{userId}";
                    var lastSaved = context.Session.GetString(sessionKey);
                    var shouldUpdate = true;

                    if (lastSaved != null && DateTime.TryParse(lastSaved, out var lastSavedTime))
                    {
                        shouldUpdate = (DateTime.UtcNow - lastSavedTime).TotalMinutes >= 2;
                    }

                    if (shouldUpdate)
                    {
                        var user = await userManager.FindByIdAsync(userId.ToString());
                        if (user != null)
                        {
                            user.LastSeenAt = DateTime.UtcNow;
                            await userManager.UpdateAsync(user);
                            context.Session.SetString(sessionKey, DateTime.UtcNow.ToString("O"));
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    public static class LastSeenMiddlewareExtensions
    {
        public static IApplicationBuilder UseLastSeen(this IApplicationBuilder app)
            => app.UseMiddleware<LastSeenMiddleware>();
    }
}
