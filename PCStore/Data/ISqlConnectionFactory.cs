using Microsoft.Data.SqlClient;

namespace PCStore.Data;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection();
    Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
