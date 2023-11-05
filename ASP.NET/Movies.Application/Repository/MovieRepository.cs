using Dapper;
using Movies.Application.Database;
using Movies.Application.Model;

namespace Movies.Application.Repository;

public class MovieRepository : IMovieRepository
{
    private readonly List<Movie> _movies = new();
    private readonly IDatabaseFactoryConnection _dbFactoryConnection;

    public MovieRepository(IDatabaseFactoryConnection dbFactoryConnection)
    {
        _dbFactoryConnection = dbFactoryConnection;
    }

    public async Task<bool> CreateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbFactoryConnection.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();
        
        var result = await connection.ExecuteAsync(new CommandDefinition("""
            insert into movies (id, title, slug, yearofrelease)
            values (@Id, @Title, @Slug, @YearOfRelease)
        """, movie, cancellationToken: token));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                insert into genres (movieid, name)
                values (@MovieId, @Name)
            """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));   
        }
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbFactoryConnection.CreateConnectionAsync();

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition("""
            select *
            from movies
            where id = @id
        """, new { id }, cancellationToken: token));

        if (movie == null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
            select name
            from genres
            where movieid = @id
        """, new { id }, cancellationToken: token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken token = default)
    {
        using var connection = await _dbFactoryConnection.CreateConnectionAsync();

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition("""
            select *
            from movies
            where slug = @slug
        """, new { slug }, cancellationToken: token));

        if (movie == null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(new CommandDefinition("""
            select name
            from genres
            where movieid = @Id
        """, new { Id = movie.Id }, cancellationToken: token));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken token = default)
    {
        using var connection = await _dbFactoryConnection.CreateConnectionAsync();

        var movies = await connection.QueryAsync(new CommandDefinition("""
            select m.*, string_agg(g.name, ',') as genres
            from movies m 
            left join genres g on m.id = g.movieid
            group by id
        """, cancellationToken: token));

        return movies.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<bool> UpdateAsync(Movie movie, CancellationToken token = default)
    {
        using var connection = await _dbFactoryConnection.CreateConnectionAsync();
        using var transition = connection.BeginTransaction();

        await connection.ExecuteAsync(new CommandDefinition("""
            delete from genres where movieid = @id
        """, new { id = movie.Id }, cancellationToken: token));
        
        var result = await connection.ExecuteAsync(new CommandDefinition("""
            update movies
            set title=@Title, slug=@Slug, yearofrelease=@YearOfRelease
            where id = @Id
        """, movie, cancellationToken: token));

        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition("""
                insert into genres (movieid, name)
                values (@MovieId, @Name)
            """, new { MovieId = movie.Id, Name = genre }, cancellationToken: token));
        }

        transition.Commit();
        return result > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbFactoryConnection.CreateConnectionAsync();
        using var transition = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new CommandDefinition("""
            delete from genres where movieid = @id
        """, new { id }, cancellationToken: token));

        var result = await connection.ExecuteAsync(new CommandDefinition("""
            delete from movies where id = @id
        """, new { id }, cancellationToken: token));
        
        transition.Commit();
        return result > 0;
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken token = default)
    {
        using var connection = await _dbFactoryConnection.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
            select count(1)
            from movies
            where id = @id
        """, new { id }, cancellationToken: token));
    }
}