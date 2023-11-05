namespace Movies.Contracts.V1.Responses;

public class ValidationMovieResponse
{
    public required IEnumerable<ValidationMovie> Errors { get; init; }
}

public class ValidationMovie
{
    public required string PropertyName { get; init; }
    public required string Message { get; init; }
}