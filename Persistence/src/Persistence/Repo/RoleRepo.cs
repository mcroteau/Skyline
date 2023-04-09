
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
    public class RoleRepo{

        public RoleRepo(){}

        public RoleRepo(DataTransferObject dto){}

        public int save(UserRole userRole){
            var connection = new SQLiteConnection("Data Source=system.db;Version=3;New=False");
            connection.Open();
            
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                INSERT INTO user_roles (user_id, role_id)
                VALUES ($user_id, $role_id)
            ";
            command.Parameters.AddWithValue("$user_id", userRole.getUserId());
            command.Parameters.AddWithValue("$role_id", userRole.getRoleId());
            command.ExecuteNonQuery();

            command.CommandText =
            @"
                SELECT last_insert_rowid()
            ";
            long id = (long)command.ExecuteScalar();
            
            return Convert.ToInt32(id);
        }    
    }
}