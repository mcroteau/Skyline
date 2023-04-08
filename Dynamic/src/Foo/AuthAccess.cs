using System;
using System.Collections;

using Skyline;
using Skyline.Model;
using Skyline.Implement;

using LiteDB;

using Foo.Model;
using Foo.Repo;

namespace Foo{

    public class AuthAccess : SecurityAccess{
        
        PersonRepo personRepo;
        DataTransferObject dto;

        public AuthAccess(){}

        public AuthAccess(DataTransferObject dto){
            this.personRepo = new PersonRepo(dto);
        }
        
        public String getPassword(String email){
            User user = personRepo.getEmail(email);
            if(user != null){
                return user.getPassword();
            }
            return "";
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
    }
}

