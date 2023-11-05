namespace Movies.Contracts.V1.Responses;

public class MovieResponse
{
    public required Guid Id { get; set; }
    
    public required string Title { get; init; }
    
    public required string Slug { get; init; }
    
    public required int YearOfRelease { get; init; }

    public required IEnumerable<string> Genres { get; init; } = Enumerable.Empty<string>();
}