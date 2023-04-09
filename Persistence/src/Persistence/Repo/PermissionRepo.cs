
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
    public class PermissionRepo{

        public PermissionRepo(){}

        public PermissionRepo(DataTransferObject dto){}

        public int save(Permission permission){
            var connection = new SQLiteConnection("Data Source=app.db;Version=3;New=False");
            connection.Open();
            
            var command = connection.CreateCommand();

            command.CommandText =
            @"
                INSERT INTO user_permissions (user_id, permission)
                VALUES ($user_id, $permission)
            ";
            command.Parameters.AddWithValue("$user_id", permission.getUserId());
            command.Parameters.AddWithValue("$permission", permission.getPermission());
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