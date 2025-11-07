namespace CollabDoc.Infrastructure.Settings;

public class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string DocumentsCollection { get; set; } = "Documents";
    public string UsersCollection { get; set; } = "Users";

}
