
using System;
using System.Collections;
using System.Collections.Generic;

using Skyline;
using Skyline.Model;
using Skyline.Annotation;

using System.Data.SQLite;

using Foo.Model;

namespace Foo.Repo{

    [Repository]
    public class PersonRepo{

        public PersonRepo(){}

        public PersonRepo(DataTransferObject dto){}

        public long insert(User user){
            var connection = new SQLiteConnection("Data Source=app.db");
            connection.Open();
            
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                SELECT last_insert_rowid()
            ";
            long id = (long)command.ExecuteScalar();

            command.CommandText =
            @"
                INSERT INTO user (id, email, password)
                VALUES ($name, $password)
            ";
            command.Parameters.AddWithValue("$id", id);
            command.Parameters.AddWithValue("$email", user.getEmail());
            command.Parameters.AddWithValue("$password", user.getPassword());
            command.ExecuteNonQuery();

            return id;
        }        


        public User get(String email){
            var connection = new SQLiteConnection("Data Source=app.db");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT id, email, password
                FROM users
                WHERE email = '$email'
            ";
            command.Parameters.AddWithValue("$email", email);

            ArrayList users = new ArrayList();
            var reader = command.ExecuteReader();
            while (reader.Read()){
                User user = new User();
                user.setId(long.Parse(reader.GetString(0)));
                user.setEmail(reader.GetString(1));
                user.setPassword(reader.GetString(2));
                users.Add(user);
            }
            return users[0] as User;
        }

    }

}