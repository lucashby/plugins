using System.Reflection;

using Microsoft.Data.SqlClient;

using PluginShared;

namespace SqlPlugin;

public class SqlPlugin : ISqlPlugin
{
    private SqlConnection? _connection;

    public SqlPlugin()
    {
        AppContext.SetSwitch("Switch.Microsoft.Data.SqlClient.UseManagedNetworkingOnWindows", true);

        _connection = new SqlConnection(@"Data Source=(localdb)\mssqllocaldb;Integrated Security=True");
    }

    public bool Disposed { get; private set; }

    public void DoWork()
    {
        try
        {
            _connection.Open();
        }
        catch (SqlException ex) when (ex.Number == -2)  // -2 means SQL timeout
        {
            // When running the test in Azure DevOps build pipeline, we'll get a SqlException with "Connection Timeout Expired".
            // We can ignore this safely in unit tests.
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Disposed) return;

        if (disposing)
        {
            SqlConnection.ClearAllPools();
            _connection?.Dispose();
            _connection = null;
        }

        Disposed = true;
    }
}
