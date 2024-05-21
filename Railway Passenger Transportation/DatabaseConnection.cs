using Npgsql;
using System.Data;

namespace Railway_Passenger_Transportation
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;
        private NpgsqlConnection _connection;

        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        public void CloseConnection()
        {
            if(_connection != null && _connection.State==ConnectionState.Open) 
            {
                _connection.Close();
            }
        }
    }
}
