using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Balenthiran.Habits.Tests;

/// <summary>
/// End-to-end tests of the WebApi routes (design MVP 1 &amp; 2) against the real host with an
/// in-memory database: exercises routing, model binding, status codes and JSON serialization.
/// The domain rules themselves are covered by the service/calculator tests; these assert the
/// HTTP surface behaves — right shapes out, right codes for missing/soft-deleted resources.
/// </summary>
public class HabitApiTests
{
    private const int Boolean = 0;
    private const int Numeric = 1;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Get_habits_returns_the_seeded_starters()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var habits = await client.GetFromJsonAsync<List<HabitDto>>("/api/habits", Json);

        Assert.NotNull(habits);
        Assert.Equal(4, habits!.Count); // Sleep / Exercise / Read / Water (design A5)
        Assert.Equal(habits.OrderBy(h => h.SortOrder).Select(h => h.Id), habits.Select(h => h.Id));
    }

    [Fact]
    public async Task Create_then_get_round_trips_the_habit()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var post = await client.PostAsJsonAsync("/api/habits", new { name = "Meditate", type = Boolean });
        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var created = await post.Content.ReadFromJsonAsync<HabitDto>(Json);
        Assert.NotNull(created);
        Assert.Equal($"/api/habits/{created!.Id}", post.Headers.Location?.ToString());

        var fetched = await client.GetFromJsonAsync<HabitDto>($"/api/habits/{created.Id}", Json);
        Assert.Equal("Meditate", fetched!.Name);
        Assert.Equal(Boolean, fetched.Type);
    }

    [Fact]
    public async Task Get_unknown_habit_is_404()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/habits/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_changes_the_habit()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();
        var created = await CreateHabitAsync(client, "Walk", Numeric, unit: "steps", target: 8000);

        var put = await client.PutAsJsonAsync(
            $"/api/habits/{created.Id}", new { name = "Walk 10k", type = Numeric, unit = "steps", target = 10000 });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var updated = await put.Content.ReadFromJsonAsync<HabitDto>(Json);
        Assert.Equal("Walk 10k", updated!.Name);
        Assert.Equal(10000, updated.Target);
    }

    [Fact]
    public async Task Update_unknown_habit_is_404()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var put = await client.PutAsJsonAsync("/api/habits/999999", new { name = "Nope", type = Boolean });

        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
    }

    [Fact]
    public async Task Archive_hides_the_habit_but_keeps_it_retrievable()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();
        var created = await CreateHabitAsync(client, "Journal", Boolean);

        var delete = await client.DeleteAsync($"/api/habits/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var active = await client.GetFromJsonAsync<List<HabitDto>>("/api/habits", Json);
        Assert.DoesNotContain(active!, h => h.Id == created.Id);

        var all = await client.GetFromJsonAsync<List<HabitDto>>("/api/habits?includeArchived=true", Json);
        Assert.Contains(all!, h => h.Id == created.Id && h.IsArchived);
    }

    [Fact]
    public async Task Archive_unknown_habit_is_404()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var delete = await client.DeleteAsync("/api/habits/999999");

        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Reorder_changes_the_display_order()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();
        var before = await client.GetFromJsonAsync<List<HabitDto>>("/api/habits", Json);
        var reversed = before!.Select(h => h.Id).Reverse().ToList();

        var put = await client.PutAsJsonAsync("/api/habits/reorder", reversed);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var after = await client.GetFromJsonAsync<List<HabitDto>>("/api/habits", Json);
        Assert.Equal(reversed, after!.Select(h => h.Id).ToList());
    }

    [Fact]
    public async Task Get_day_lists_every_habit_uncompleted_by_default()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var day = await client.GetFromJsonAsync<JsonElement>("/api/days/2026-07-10", Json);

        Assert.Equal("2026-07-10", day.GetProperty("date").GetString());
        var habits = day.GetProperty("habits");
        Assert.Equal(4, habits.GetArrayLength());
        Assert.All(habits.EnumerateArray(), h => Assert.False(h.GetProperty("isComplete").GetBoolean()));
    }

    [Fact]
    public async Task Upsert_entry_logs_a_value_and_completes_a_boolean_habit()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();
        var habit = await CreateHabitAsync(client, "Floss", Boolean);
        const string date = "2026-07-10";

        var put = await client.PutAsJsonAsync("/api/entries", new { habitId = habit.Id, date, value = 1.0 });
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);

        var day = await client.GetFromJsonAsync<JsonElement>($"/api/days/{date}", Json);
        var slot = day.GetProperty("habits").EnumerateArray().Single(h => h.GetProperty("habitId").GetInt32() == habit.Id);
        Assert.Equal(1.0, slot.GetProperty("value").GetDouble());
        Assert.True(slot.GetProperty("isComplete").GetBoolean());
    }

    [Fact]
    public async Task Upsert_entry_for_unknown_habit_is_404()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var put = await client.PutAsJsonAsync(
            "/api/entries", new { habitId = 999999, date = "2026-07-10", value = 1.0 });

        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
    }

    [Fact]
    public async Task History_returns_a_grid_of_the_requested_length()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();
        var habit = await CreateHabitAsync(client, "Pushups", Boolean);

        var history = await client.GetFromJsonAsync<JsonElement>(
            $"/api/habits/{habit.Id}/history?historyDays=7&today=2026-07-10", Json);

        Assert.Equal(habit.Id, history.GetProperty("habitId").GetInt32());
        Assert.Equal(7, history.GetProperty("days").GetArrayLength());
    }

    [Fact]
    public async Task History_for_unknown_habit_is_404()
    {
        using var factory = new HabitApiFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/habits/999999/history");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<HabitDto> CreateHabitAsync(
        HttpClient client, string name, int type, string? unit = null, double? target = null)
    {
        var response = await client.PostAsJsonAsync("/api/habits", new { name, type, unit, target });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HabitDto>(Json))!;
    }

    private sealed record HabitDto(
        int Id, string Name, int Type, string? Unit, double? Target, int SortOrder, bool IsArchived);
}
