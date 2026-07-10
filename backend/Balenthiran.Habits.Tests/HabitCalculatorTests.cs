using Balenthiran.Habits.Abstractions.Enums;
using Balenthiran.Habits.Services;

namespace Balenthiran.Habits.Tests;

/// <summary>
/// The pure completion + streak rules (design A3/A4). No database, no clock — this is the
/// logic that decides the app's sense of momentum, so it is tested directly and exhaustively
/// (mirrors language-vocab's PoolMathTests).
/// </summary>
public class HabitCalculatorTests
{
    // A fixed "today" so the multi-day fixtures read as a calendar.
    private static readonly DateOnly Today = new(2026, 1, 15);
    private static DateOnly Ago(int days) => Today.AddDays(-days);

    // ---- IsComplete (A3) --------------------------------------------------

    [Theory]
    // Boolean: ticked (non-zero) completes; unticked / missing does not.
    [InlineData(HabitType.Boolean, null, 1.0, true)]
    [InlineData(HabitType.Boolean, null, 0.0, false)]
    [InlineData(HabitType.Boolean, null, null, false)]
    // Numeric with a target: value >= target completes.
    [InlineData(HabitType.Numeric, 8.0, 8.0, true)]
    [InlineData(HabitType.Numeric, 8.0, 9.5, true)]
    [InlineData(HabitType.Numeric, 8.0, 7.9, false)]
    [InlineData(HabitType.Numeric, 8.0, null, false)]
    // Numeric without a target: any logged value completes, even zero.
    [InlineData(HabitType.Numeric, null, 3.0, true)]
    [InlineData(HabitType.Numeric, null, 0.0, true)]
    [InlineData(HabitType.Numeric, null, null, false)]
    public void IsComplete_matches_the_rule(HabitType type, double? target, double? value, bool expected)
        => Assert.Equal(expected, HabitCalculator.IsComplete(type, target, value));

    // ---- CurrentStreak (A4) ----------------------------------------------

    [Fact]
    public void CurrentStreak_counts_today_and_back_when_today_is_complete()
    {
        var complete = new HashSet<DateOnly> { Today, Ago(1), Ago(2) };
        Assert.Equal(3, HabitCalculator.CurrentStreak(complete, Today));
    }

    [Fact]
    public void CurrentStreak_treats_today_as_a_grace_day_when_not_yet_done()
    {
        // Today not logged yet, but the three days before it are complete — streak holds.
        var complete = new HashSet<DateOnly> { Ago(1), Ago(2), Ago(3) };
        Assert.Equal(3, HabitCalculator.CurrentStreak(complete, Today));
    }

    [Fact]
    public void CurrentStreak_is_zero_when_today_and_yesterday_both_missed()
    {
        var complete = new HashSet<DateOnly> { Ago(2), Ago(3) };
        Assert.Equal(0, HabitCalculator.CurrentStreak(complete, Today));
    }

    [Fact]
    public void CurrentStreak_stops_at_the_first_gap()
    {
        // Today + yesterday complete, then a gap at Ago(2); Ago(3)/Ago(4) don't count.
        var complete = new HashSet<DateOnly> { Today, Ago(1), Ago(3), Ago(4) };
        Assert.Equal(2, HabitCalculator.CurrentStreak(complete, Today));
    }

    [Fact]
    public void CurrentStreak_is_zero_with_no_history()
        => Assert.Equal(0, HabitCalculator.CurrentStreak(new HashSet<DateOnly>(), Today));

    // ---- LongestStreak (A4) ----------------------------------------------

    [Fact]
    public void LongestStreak_picks_the_longest_consecutive_run()
    {
        // Runs of 2 (Ago 9,8), 4 (Ago 6,5,4,3) and 1 (Ago 1): longest is 4.
        var complete = new List<DateOnly>
        {
            Ago(9), Ago(8),
            Ago(6), Ago(5), Ago(4), Ago(3),
            Ago(1),
        };
        Assert.Equal(4, HabitCalculator.LongestStreak(complete));
    }

    [Fact]
    public void LongestStreak_handles_a_single_unbroken_run()
    {
        var complete = new List<DateOnly> { Ago(2), Ago(1), Today };
        Assert.Equal(3, HabitCalculator.LongestStreak(complete));
    }

    [Fact]
    public void LongestStreak_ignores_duplicate_and_unordered_dates()
    {
        var complete = new List<DateOnly> { Today, Ago(2), Ago(1), Ago(1), Today };
        Assert.Equal(3, HabitCalculator.LongestStreak(complete));
    }

    [Fact]
    public void LongestStreak_is_zero_with_no_history()
        => Assert.Equal(0, HabitCalculator.LongestStreak([]));
}
