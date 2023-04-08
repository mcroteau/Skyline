
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

        public long save(User user){
            var connection = new SQLiteConnection("Data Source=app.db;Version=3;New=False");
            connection.Open();
            var transaction = connection.BeginTransaction();
            
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                INSERT INTO users (email, password)
                VALUES ($email, $password)
            ";
            command.Parameters.AddWithValue("$email", user.getEmail());
            command.Parameters.AddWithValue("$password", user.getPassword());
            command.ExecuteNonQuery();

            command.CommandText =
            @"
                SELECT last_insert_rowid()
            ";
            long id = (long)command.ExecuteScalar();
            
            transaction.Commit();
            connection.Close();
            
            return id;
        }        


        public User getEmail(String email){
            var connection = new SQLiteConnection("Data Source=app.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            Console.WriteLine("za:" + email);



// command.CommandText = "SELECT * FROM users";

//          var sqlite_datareader = command.ExecuteReader();
//          while (sqlite_datareader.Read())
//          {
//             var myreader = sqlite_datareader.GetString(1);
//             Console.WriteLine("z" + myreader);
//          }


            command.CommandText =
            @"
                SELECT id, email, password
                FROM users WHERE email = $email
            ";
            command.Parameters.Add(new SQLiteParameter("$email", email));

            ArrayList users = new ArrayList();
            var reader = command.ExecuteReader();
            while (reader.Read()){
                User user = new User();
                Console.WriteLine("read:");
                user.setId(reader.GetInt32(0));
                user.setEmail(reader.GetString(1));
                user.setPassword(reader.GetString(2));
                users.Add(user);
            }
            Console.WriteLine("zq:" + users.Count);
            
            connection.Close();
            
            if(users.Count > 0){
                return users[0] as User;
            }
            
            return null;
        }


        public HashSet<String> getRoles(long userId){
            var connection = new SQLiteConnection("Data Source=app.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT description    
                FROM user_roles outer join on user_roles.role_id = roles.id
                WHERE user_id = '$userId'
            ";
            command.Parameters.AddWithValue("$userId", userId);

            HashSet<String> userRoles = new HashSet<String>();
            var reader = command.ExecuteReader();
            while (reader.Read()){
                userRoles.Add(reader.GetString(0));
            }
            connection.Close();
            
            return userRoles;
        }
        
        public HashSet<String> getPermissions(long userId){
            var connection = new SQLiteConnection("Data Source=app.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT permission    
                FROM permissions
                WHERE user_id = '$userId'
            ";
            command.Parameters.AddWithValue("$userId", userId);

            HashSet<String> permissions = new HashSet<String>();
            var reader = command.ExecuteReader();
            while (reader.Read()){
                permissions.Add(reader.GetString(0));
            }

            connection.Close();
            
            return permissions;
        }
    }

}