namespace JahnDigital.StudentBank.Infrastructure.Persistence;

/// <summary>
///     Contract to describe methods that initialize and seed data stores.
/// </summary>
public interface IDbInitializerService
{
    /// <summary>
    ///     Ensure the database is created and migrated.
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Seed initial data, and test data if in a development environment.
    /// </summary>
    void SeedData();
}
