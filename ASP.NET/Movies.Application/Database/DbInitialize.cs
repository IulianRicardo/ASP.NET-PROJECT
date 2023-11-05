using Dapper;

namespace Movies.Application.Database;

public class DbInitialize
{
    private readonly IDatabaseFactoryConnection _databaseFactoryConnection;

    public DbInitialize(IDatabaseFactoryConnection databaseFactoryConnection)
    {
        _databaseFactoryConnection = databaseFactoryConnection;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _databaseFactoryConnection.CreateConnectionAsync();

        await connection.ExecuteAsync("""
            create table if not exists Movies (
                id uuid primary key,
                title text not null,
                slug text not null,
                yearofrelease integer not null
            );
        """);

        await connection.ExecuteAsync("""
            create index concurrently if not exists movies_slug_idx
            on movies
            using btree(slug)
        """);

        await connection.ExecuteAsync("""
            create table if not exists Genres (
                movieid uuid references movies (id),
                name text not null
            );
        """);
    }
}