using System;
using System.Collections;
using Skyline;
using Skyline.Implement;

using LiteDB;

using Foo.Model;

namespace Foo{

    public class AuthAccess : SecurityAccess{
        
        DataTransferObject dto;
        LiteDatabase db;

        public AuthAccess(){}

        public AuthAccess(DataTransferObject dto){
            this.dto = dto;
            this.db = new LiteDatabase(@"Ocean.db");
        }
        
        public String getPassword(String email){
            String password = getUserPassword(email);
            return password;
        }

        public HashSet<String> getRoles(String email){
            // var users = db.GetCollection<User>();
            // User user = users.Queryy<User>().Where("select * from users where email = '" + email + "'");
            // var userRolesList = users.Queryy<User>().Where("select * from user_roles where user_id = " + user.getId());
            // HashSet<String> userRoles = new HashSet<String>();
            // foreach(UserRole userRole in userRoles){
            //     var userRole = db.Query("select * from roles where id = " + userRole.getId());
            //     userRoles.Add(userRole.getDescription());
            // }
            // Console.WriteLine("ur:" + userRoles.Length);
            // return userRoles;
            return new HashSet<String>();
        }

        public HashSet<String> getPermissions(String email){
            // var users = db.GetCollection<User>();
            // User user = users.Queryy<User>().Where("select * from users where email = '" + email + "'");
            // var userPermissions = users.Queryy<User>().Where("select permission from user_permissions where user_id = " + user.getId());
            // Console.WriteLine("u:" + userPermissions.Length);
            // return new HashSet<String>(userPermissions);
            return new HashSet<String>();
        }

        String getUserPassword(String email){
            var bsonReader = db.Execute("select $ from users where email = '" + email + "'");
            ArrayList output = new ArrayList();
            while (bsonReader.Read())output.Add(bsonReader.Current);
            if(output.Count > 0){
                var userBson = output[0];
                User user = new User();
                Console.WriteLine(userBson);
                return user.getPassword();
            }
            return "";
        }
    }
}

