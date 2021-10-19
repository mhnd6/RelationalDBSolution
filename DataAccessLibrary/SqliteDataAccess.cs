using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SQLite;
using Dapper;
using System.Linq;

namespace DataAccessLibrary
{
    public class SqliteDataAccess
    {
        public List<T> LoadData<T, U>(string sqlStatment, U parameters, string connectionString)
        {
            using (IDbConnection connection = new SQLiteConnection(connectionString))
            {
                List<T> rows = connection.Query<T>(sqlStatment, parameters).ToList();
                return rows;
            }
        }

        public void SaveData<T>(string sqlStatment, T parameters, string connectionString)
        {
            using (IDbConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Execute(sqlStatment, parameters);
            }
        }
    }
}
