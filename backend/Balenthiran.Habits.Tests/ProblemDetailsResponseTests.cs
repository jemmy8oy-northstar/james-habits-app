using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Balenthiran.Habits.Tests;

/// <summary>
/// End-to-end proof that the global exception handler (issue #7) shapes a thrown
/// <c>NotFoundException</c> into an RFC 7807 <c>ProblemDetails</c> response: the right
/// content type, status, machine-readable <c>errorCode</c>, and a caller-safe detail.
/// </summary>
public class ProblemDetailsResponseTests
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Not_found_returns_a_problem_details_body_with_error_code()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/habits/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("not_found", problem.GetProperty("errorCode").GetString());
        // Anticipated failures surface their own (developer-authored) message.
        Assert.Contains("999999", problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Upsert_for_unknown_habit_also_yields_the_not_found_problem_code()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var response = await client.PutAsJsonAsync(
            "/api/entries", new { habitId = 424242, date = "2026-07-11", value = 1.0 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.Equal("not_found", problem.GetProperty("errorCode").GetString());
    }
}
