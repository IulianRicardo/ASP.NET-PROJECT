using System.Text.RegularExpressions;

namespace Movies.Application.Model;

public partial class Movie
{
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }

    public string Slug => GenerateSlug();
    
    public required int YearOfRelease { get; set; }

    public required List<string> Genres { get; set; } = new();

    private string GenerateSlug()
    {
        var sluggerTitle = SlugRegex().Replace(Title, string.Empty)
            .ToLower().Replace(" ", "-");

        return $"{sluggerTitle}-{YearOfRelease}";
    }

    [GeneratedRegex("[^0-9A-Za-z _-]", RegexOptions.NonBacktracking, 5)]
    private partial Regex SlugRegex();
}