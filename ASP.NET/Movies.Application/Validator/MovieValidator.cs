using FluentValidation;
using Movies.Application.Model;
using Movies.Application.Repository;

namespace Movies.Application.Validator;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;

    public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;

        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.YearOfRelease)
            .LessThanOrEqualTo(DateTime.UtcNow.Year);

        RuleFor(x => x.Genres)
            .NotEmpty();

        RuleFor(x => x.Slug)
            .MustAsync(ValidatorSlug)
            .WithMessage("This movie already exists in the system");
    }

    private async Task<bool> ValidatorSlug(Movie movie, string slug, CancellationToken token = default)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug);

        if (existingMovie is not null)
        {
            return existingMovie.Id == movie.Id;
        }

        return existingMovie is null;
    }
}