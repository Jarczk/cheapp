namespace Cheapp.Options;
public record MongoOptions
{
    public string ConnectionString { get; init; } = "mongodb://localhost:27017";
    public string Database { get; init; } = "Cheapp";
}