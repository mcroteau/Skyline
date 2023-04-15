using System;

using System.Data.SQLite;

public class DatabaseSetup{

    SQLiteConnection connection;

    public DatabaseSetup(){
        this.connection = new SQLiteConnection("Data Source=system.db;Version=3;New=True");
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

            insert into roles (id, description) values (1, 'super-role');
            insert into roles (id, description) values (2, 'default-role');

            insert into users (id, email, password) values (1, 'super@plsar.net', 'effort.');
            insert into user_roles (id, user_id, role_id) values (1, 1, 1);
            insert into user_permissions (id, user_id, permission) values (1, 1, 'users:maintenance:1');

            insert into users (id, email, password) values (2, 'abc@plsar.net', 'effort.');
            insert into user_roles (id, user_id, role_id) values (2, 2, 2);
            insert into user_permissions (id, user_id, permission) values (2, 2, 'users:maintenance:2');

            insert into users (id, email, password) values (3, 'def@plsar.net', 'effort.');
            insert into user_roles (id, user_id, role_id) values (3, 3, 2);
            insert into user_permissions (id, user_id, permission) values (3, 3, 'users:maintenance:3');

            insert into users (id, email, password) values (4, 'ghi@plsar.net', 'effort.');
            insert into user_roles (id, user_id, role_id) values (4, 4, 2);
            insert into user_permissions (id, user_id, permission) values (4, 4, 'users:maintenance:4');

            insert into users (id, email, password) values (5, 'jkl@plsar.net', 'effort.');
            insert into user_roles (id, user_id, role_id) values (5, 5, 2);
            insert into user_permissions (id, user_id, permission) values (5, 5, 'users:maintenance:5');
        ";
        command.ExecuteNonQuery();

        connection.Close();
    }    
    public DatabaseSetup clean(){
        File.Delete("system.db");
        return this;
    }
    public SQLiteConnection getConnection(){
        return this.connection;
    }
}