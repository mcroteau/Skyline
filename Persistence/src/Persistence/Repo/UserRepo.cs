
using System;
using System.Collections;
using System.Collections.Generic;

using Skyline;
using Skyline.Model;
using Skyline.Annotation;

using System.Data.SQLite;

using Persistence.Model;

namespace Persistence.Repo{

    [Repository]
    public class UserRepo{

        public UserRepo(){}

        public UserRepo(DataTransferObject dto){}

        public User getId(int id){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            
            command.CommandText =
            @"
                SELECT id, email, password
                FROM users WHERE id = $id
            ";
            command.Parameters.Add(new SQLiteParameter("$id", id));

            ArrayList users = new ArrayList();
            var reader = command.ExecuteReader();
            while (reader.Read()){
                User user = new User();
                user.setId(reader.GetInt32(0));
                user.setEmail(reader.GetString(1));
                user.setPassword(reader.GetString(2));
                users.Add(user);
            }
            
            connection.Close();
            
            if(users.Count > 0){
                return users[0] as User;
            }
            
            return null;
        }


        public User getEmail(String email){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            
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
                user.setId(reader.GetInt32(0));
                user.setEmail(reader.GetString(1));
                user.setPassword(reader.GetString(2));
                users.Add(user);
            }
            
            connection.Close();
            
            if(users.Count > 0){
                return users[0] as User;
            }
            
            return null;
        }


        public ArrayList getList(){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            
            command.CommandText =
            @"
                SELECT * FROM users
            ";

            ArrayList users = new ArrayList();
            var reader = command.ExecuteReader();
            while (reader.Read()){
                User user = new User();
                user.setId(reader.GetInt32(0));
                user.setEmail(reader.GetString(1));
                user.setPassword(reader.GetString(2));
                users.Add(user);
            }

            connection.Close();
            
            return users;
        }

        public int save(User user){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();
            
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
            
            return Convert.ToInt32(id);
        }      

        public void update(User user){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();
            
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                update users set email = $email, password = $password where id = $id
            ";
            command.Parameters.AddWithValue("$id", user.getId());
            command.Parameters.AddWithValue("$email", user.getEmail());
            command.Parameters.AddWithValue("$password", user.getPassword());
            command.ExecuteNonQuery();
        }

        public void delete(int id){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();
            
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                delete from users where id = $id
            ";
            command.Parameters.AddWithValue("$id", id);
            command.ExecuteNonQuery();
        }

        public HashSet<String> getRoles(User user){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT description    
                FROM user_roles inner join roles on user_roles.role_id = roles.id
                WHERE user_id = $userId
            ";
            command.Parameters.AddWithValue("$userId", user.getId());

            HashSet<String> userRoles = new HashSet<String>();
            var reader = command.ExecuteReader();
            while (reader.Read()){
                userRoles.Add(reader.GetString(0));
            }
            connection.Close();
            
            return userRoles;
        }
        
        public HashSet<String> getPermissions(User user){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT permission    
                FROM user_permissions 
                WHERE user_id = $userId
            ";
            command.Parameters.AddWithValue("$userId", user.getId());

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