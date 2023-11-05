using FluentValidation;
using Movies.Contracts.V1.Responses;

namespace Movies.Api.Mapping;

public class ValidateMappingMiddleware
{
    private readonly RequestDelegate _next;

    public ValidateMappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;

            var validationMovieResponse = new ValidationMovieResponse
            {
                Errors = ex.Errors.Select(x => new ValidationMovie
                {
                    PropertyName = x.PropertyName,
                    Message = x.ErrorMessage
                })
            };

            await context.Response.WriteAsJsonAsync(validationMovieResponse);
        }
    }
}