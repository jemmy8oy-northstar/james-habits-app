using Balenthiran.Habits.Abstractions.Exceptions;
using Balenthiran.Habits.WebApi.ExceptionHandling;

namespace Balenthiran.Habits.Tests;

/// <summary>
/// The pure exception-to-HTTP mapping (issue #7): every <see cref="AppException"/> subclass
/// resolves to its documented status, and any unexpected fault becomes a non-exposing 500.
/// </summary>
public class ExceptionResultTests
{
    public static readonly TheoryData<AppException, int, string> KnownMappings = new()
    {
        { new NotFoundException("x"), 404, "not_found" },
        { new ValidationException("x"), 400, "invalid_input" },
        { new ConflictException("x"), 409, "conflict" },
        { new UnauthorizedException("x"), 401, "unauthenticated" },
        { new ForbiddenException("x"), 403, "forbidden" },
        { new UpstreamServiceException("x"), 502, "upstream_failure" },
    };

    [Theory]
    [MemberData(nameof(KnownMappings))]
    public void Each_app_exception_maps_to_its_status_and_code(AppException exception, int status, string errorCode)
    {
        var result = ExceptionResult.Resolve(exception);

        Assert.Equal(status, result.StatusCode);
        Assert.Equal(errorCode, result.ErrorCode);
        // Anticipated failures surface their own message.
        Assert.True(result.ExposeMessage);
    }

    [Fact]
    public void Unexpected_exception_is_a_non_exposing_500()
    {
        var result = ExceptionResult.Resolve(new InvalidOperationException("secret internal detail"));

        Assert.Equal(500, result.StatusCode);
        Assert.Equal("internal_error", result.ErrorCode);
        // A 500 must NOT leak the original message to the caller.
        Assert.False(result.ExposeMessage);
    }
}
