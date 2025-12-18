using System;
using System.Data;
using System.Data.OleDb;
using System.IO;

namespace LibraryManagementSystem.ViewModel
{
    /// <summary>
    /// Base class for database operations - handles OleDb connection to Access database
    /// </summary>
    public class BaseDB
    {
        private static string _connectionString;
        private OleDbConnection _connection;

        /// <summary>
        /// Gets the connection string for the Access database
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    // Use the actual database location
                    string dbPath = @"C:\_Lior_\School\project\þþþþLibraryManagmentCS_5\database\LibraryManagement.accdb";
                    
                    _connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dbPath};Persist Security Info=False;";
                }
                return _connectionString;
            }
            set => _connectionString = value;
        }

        /// <summary>
        /// Gets the current database connection
        /// </summary>
        protected OleDbConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new OleDbConnection(ConnectionString);
                }
                return _connection;
            }
        }

        /// <summary>
        /// Opens the database connection
        /// </summary>
        protected void OpenConnection()
        {
            try
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to open database connection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Closes the database connection
        /// </summary>
        protected void CloseConnection()
        {
            try
            {
                if (Connection.State == ConnectionState.Open)
                {
                    Connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to close database connection: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executes a non-query SQL command (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>Number of rows affected</returns>
        protected int ExecuteNonQuery(string query, params OleDbParameter[] parameters)
        {
            try
            {
                OpenConnection();
                using (OleDbCommand command = new OleDbCommand(query, Connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute non-query: {ex.Message}", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Executes a query and returns a DataTable
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>DataTable with results</returns>
        protected DataTable ExecuteQuery(string query, params OleDbParameter[] parameters)
        {
            try
            {
                OpenConnection();
                using (OleDbCommand command = new OleDbCommand(query, Connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute query: {ex.Message}", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Executes a scalar query (returns single value)
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <param name="parameters">Optional parameters</param>
        /// <returns>Single value result</returns>
        protected object ExecuteScalar(string query, params OleDbParameter[] parameters)
        {
            try
            {
                OpenConnection();
                using (OleDbCommand command = new OleDbCommand(query, Connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute scalar query: {ex.Message}", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection successful</returns>
        public bool TestConnection()
        {
            try
            {
                OpenConnection();
                return Connection.State == ConnectionState.Open;
            }
            catch
            {
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Disposes the database connection
        /// </summary>
        public void Dispose()
        {
            if (_connection != null)
            {
                CloseConnection();
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
