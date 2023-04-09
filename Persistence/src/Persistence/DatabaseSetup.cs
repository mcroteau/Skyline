using System;

using System.Data.SQLite;

namespace Persistence{

    public class DatabaseSetup{
    
        SQLiteConnection connection;

        public DatabaseSetup(){
            this.connection = new SQLiteConnection("Data Source=app.db;Version=3;New=True");
        }

        public void setup(){
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
                CREATE TABLE user_permissions (
                    id integer not null primary key autoincrement,
                    user_id integer,
                    permission text not null
                );
                insert into users (id, email, password) values (1, 'abc@plsar.net', 'effort.');
                insert into roles (id, description) values (1, 'super-role');
                insert into user_roles (id, user_id, role_id) values (1, 1, 1);
                insert into user_permissions (id, user_id, permission) values (1, 1, 'users:maintenance:1');
            ";
            command.ExecuteNonQuery();

            connection.Close();
        }    
        public DatabaseSetup clean(){
            File.Delete("app.db");
            return this;
        }
        public SQLiteConnection getConnection(){
            return this.connection;
        }
    }
}