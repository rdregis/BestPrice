
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;


public class DBSqlite
{

    
    public DBSqlite() 
    {        
    }
    public void dbConnection(string dbName)
    {
        string connectionString = new SqliteConnectionStringBuilder()
        {
            DataSource = dbName,
            Cache = SqliteCacheMode.Private,
            Mode = SqliteOpenMode.ReadWriteCreate,

            DefaultTimeout = 10
        
        }.ToString();
        

        try {
            this.sqliteConnection = new SqliteConnection(connectionString);
            this.sqliteConnection.Open();
        }
        catch (Exception ex) {
            throw new Exception("Error on Connection", ex);
        }

    }

    public void dbDisconnection()
    {
        try {
            this.sqliteConnection.Close();
        }
        catch (Exception ex) {
            throw new Exception("Error on Disconnection", ex);
        }

    }
    public void executeCommand(string command)
    {
        try {
            var cmd = createCommand(command);
            cmd.ExecuteNonQuery();
        }   
        catch(Exception ex) {
            throw ex;
        }
    }

    public SqliteCommand createCommand(string command) 
    {
        var cmd = sqliteConnection.CreateCommand();
        cmd.CommandText = command;

        return cmd;
    }

    public SqliteTransaction createTransaction() 
    {
        var transaction = sqliteConnection.BeginTransaction(deferred: true);
        
        return transaction;
    }

    public String getField(SqliteDataReader reader, string fieldName)
    {
        return (reader[fieldName]!.ToString()!);

    }
    private SqliteConnection sqliteConnection = null!;
}
       
// https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/?tabs=net-cli
// https://www.macoratti.net/17/04/cshp_sqlite1.htm
// https://www.codeguru.com/dotnet/using-sqlite-in-a-c-application/