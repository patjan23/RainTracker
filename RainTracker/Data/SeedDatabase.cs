using RainTracker.Models;

namespace RainTracker.Data
{
    public static class SeedDatabase
    {
        public static void Initialize(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RainContext>();

            if (!context.RainRecords.Any())
            {
                context.RainRecords.AddRange(
                    new RainRecord { Id =  Guid.NewGuid().ToString(), Rain = true, Timestamp = DateTime.UtcNow.AddMinutes(-40), UserId = "PatJan" },
                    new RainRecord { Id = Guid.NewGuid().ToString(), Rain = false, Timestamp = DateTime.UtcNow.AddMinutes(-30), UserId = "NanJan" },
                    new RainRecord { Id = Guid.NewGuid().ToString(), Rain = true, Timestamp = DateTime.UtcNow.AddMinutes(-20), UserId = "GregJan" },
                    new RainRecord { Id = Guid.NewGuid().ToString(), Rain = true, Timestamp = DateTime.UtcNow.AddMinutes(-10), UserId = "AugJan" }
                );
                context.SaveChanges();
            }
        }
    }
}
