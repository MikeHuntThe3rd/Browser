using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Browser
{
    public static class DB
    {
        public static readonly MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            UserID = "root",
            Password = "",
            Database = "users_db",
        };
        public static MySqlConnection GetConn()
        {
            var connection = new MySqlConnection(builder.ConnectionString);
            connection.Open();
            return connection;
        }
        public static List<Tuple<int, string, string, int>> GetUsers()
        {
            List<Tuple<int, string, string, int>> result = new List<Tuple<int, string, string, int>>();
            string sql = "SELECT * FROM user;";
            var cmd = new MySqlCommand(sql, GetConn());
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(Tuple.Create(reader.GetInt32("id"), reader.GetString("username"), reader.GetString("password"), reader.GetInt32("engine_id")));
            }
            return result;
        }
        public static ObservableCollection<fav> GetUserData(int id)
        {
            ObservableCollection<fav> result = new ObservableCollection<fav>();
            string sql = "SELECT * FROM data WHERE user_id = @id;";
            var cmd = new MySqlCommand(sql, GetConn());
            cmd.Parameters.AddWithValue("id", id);
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var temp = new fav()
                {
                    Title = reader.GetString("title"),
                    Icon = reader.GetString("icon"),
                    Link = reader.GetString("link"),
                };
                result.Add(temp);
            }
            return result;
        }
        public static ObservableCollection<engine> GetEngines(int id = -1)
        {
            string sql;
            ObservableCollection<engine> result = new ObservableCollection<engine>();
            var cmd = new MySqlCommand();
            cmd.Connection = GetConn();
            if (id == -1) 
            {
                sql = "SELECT * FROM engines;";
            }
            else
            {
                sql = "SELECT * FROM engines WHERE id = @id;";
                cmd.Parameters.AddWithValue("id", id);
            }
            cmd.CommandText = sql;
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var temp = new engine()
                {
                    name = reader.GetString("name"),
                    query = reader.GetString("query"),
                    id = reader.GetInt32("id")
                };
                result.Add(temp);
            }
            return result;
        }
        public static void InsertUser(Tuple<string, string, int> values)
        {
            string sql = "INSERT INTO user (username, password, engine_id) VALUES (@usr, @pass, @engine)";
            MySqlCommand cmd = new MySqlCommand(sql, GetConn());
            cmd.Parameters.AddWithValue("usr", values.Item1);
            cmd.Parameters.AddWithValue("pass", values.Item2);
            cmd.Parameters.AddWithValue("engine", values.Item3);
            cmd.ExecuteNonQuery();
        }
        public static void InsertData(Tuple<int, string, string, string> values)
        {
            string sql = "INSERT INTO data (user_id, icon, link, title) VALUES (@id, @icon, @link, @title);";
            MySqlCommand cmd = new MySqlCommand(sql, GetConn());
            cmd.Parameters.AddWithValue("id", values.Item1);
            cmd.Parameters.AddWithValue("icon", values.Item2);
            cmd.Parameters.AddWithValue("link", values.Item3);
            cmd.Parameters.AddWithValue("title", values.Item4);
            cmd.ExecuteNonQuery();
        }
        public static void UpdateEngineId(int user, int engine)
        {
            string sql = "UPDATE user SET engine_id = @newid WHERE id = @id;";
            MySqlCommand cmd = new MySqlCommand(sql, GetConn());
            cmd.Parameters.AddWithValue("id", user);
            cmd.Parameters.AddWithValue("newid", engine);
            cmd.ExecuteNonQuery();
        }
}
}
