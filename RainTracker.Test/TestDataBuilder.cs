using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RainTracker.Controllers;
using RainTracker.Models;

namespace RainTracker.Test;

/// <summary>
/// Builder class for creating test data using Bogus
/// </summary>
public static class TestDataBuilder
{
    private static readonly Faker<RainRecord> RainDataFaker = new Faker<RainRecord>()
        .RuleFor(r => r.Id, f => f.Random.Uuid().ToString())
        .RuleFor(r => r.UserId, f => f.Internet.UserName())
        .RuleFor(r => r.Rain, f => f.Random.Bool())
        .RuleFor(r => r.Timestamp, f => f.Date.Recent(30).ToUniversalTime());

    /// <summary>
    /// Creates a single RainData record
    /// </summary>
    public static RainRecord CreateRainData(string? userId = null, bool? rain = null, DateTime? timestamp = null)
    {
        var rainData = RainDataFaker.Generate();

        if (!string.IsNullOrEmpty(userId))
            rainData.UserId = userId;

        if (rain.HasValue)
            rainData.Rain = rain.Value;

        if (timestamp.HasValue)
            rainData.Timestamp = timestamp.Value;

        return rainData;
    }

    /// <summary>
    /// Creates multiple RainData records
    /// </summary>
    public static List<RainRecord> CreateRainDataList(int count, string? userId = null)
    {
        var rainDataList = new List<RainRecord>();

        for (int i = 0; i < count; i++)
        {
            var rainData = RainDataFaker.Generate();

            if (!string.IsNullOrEmpty(userId))
                rainData.UserId = userId;

            rainDataList.Add(rainData);
        }

        return rainDataList;
    }

    /// <summary>
    /// Creates RainData records for a specific user with varied timestamps
    /// </summary>
    public static List<RainRecord> CreateRainDataForUser(string userId, int count)
    {
        var rainDataList = new List<RainRecord>();
        var baseDate = DateTime.UtcNow.AddDays(-count);

        for (int i = 0; i < count; i++)
        {
            rainDataList.Add(new RainRecord
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Rain = i % 2 == 0, // Alternate between true and false
                Timestamp = baseDate.AddDays(i)
            });
        }

        return rainDataList;
    }

    /// <summary>
    /// Creates a valid RainDataRequest
    /// </summary>
    public static RainRecordDto CreateRainDataRequest(bool rain = true)
    {
        return new RainRecordDto { Rain = rain };
    }

    /// <summary>
    /// Creates test user IDs
    /// </summary>
    public static string CreateUserId(string prefix = "testuser")
    {
        return $"{prefix}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Creates multiple test user IDs
    /// </summary>
    public static List<string> CreateUserIds(int count, string prefix = "testuser")
    {
        return Enumerable.Range(1, count)
            .Select(i => $"{prefix}_{i}_{Guid.NewGuid():N}")
            .ToList();
    }

    /// <summary>
    /// Mocks the HTTP request headers with the given userId and assigns it to the controller.
    /// </summary>
    /// <param name="controller">The controller to which the mocked context will be assigned</param>
    /// <param name="userId">The userId to set in the x-userid header</param>
    public static void SetupMockHttpContext(RainController controller, string? userId)
    {
        // Create a mock HTTP context
        var mockHttpContext = new DefaultHttpContext();

        // Set the x-userid header
        mockHttpContext.Request.Headers["x-userid"] = userId;

        // Assign the mocked context to the controller
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext
        };
    }

   
}