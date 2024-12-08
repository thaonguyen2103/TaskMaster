using Microsoft.Data.SqlClient;

public static class DatabaseConnection
{
    private static string connectionString = "Server=LAPTOP-KNDSJBLQ\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(connectionString);
    }
}
public static class UserSession
{
    public static string CurrentUserID { get; set; }
    public static string CurrentProjectID { get; set; }
    public static string CurrentProjectName { get; set; }
    public static string CurrentTaskID { get; set; }
    public static string CurrentListID { get; set; }
}

