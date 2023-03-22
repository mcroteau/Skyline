using System;

using System.Data.SQLite;

namespace Foo{
    public class DatabaseSetup{
        public void setup(){
            var connection = new SQLiteConnection("Data Source=app.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE users (
                    id integer not null primary key autoincrement,
                    email text not null,
                    password text not null
                );
                CREATE TABLE roles (
                    id integer not null primary key autoincrement,
                    description text not null
                );
                CREATE TABLE user_roles (
                    id integer not null primary key autoincrement,
                    user_id integer,
                    role_id integer
                );
                CREATE TABLE permissions (
                    id integer not null primary key autoincrement,
                    user_id integer,
                    permission text not null
                );
            ";
            command.ExecuteNonQuery();
        }    
        public void clean(){
            File.Delete("app.db");
        }
    }
}