using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using Skyline;
using Skyline.Model;
using Skyline.Schemes;
using Skyline.Security;

using Foo.Model;

using LiteDB;

namespace Foo{
    class Launcher{

        public static int Main(String[] args){
            
            LiteDatabase db = new LiteDatabase(@"Ocean.db");
            
            var users = db.GetCollection<User>("users");
            var roles = db.GetCollection<Role>("roles");
            var userRoles = db.GetCollection<UserRole>("user_roles");
            var permissions = db.GetCollection<Permission>("permissions");
            
            users.DeleteMany("1=1");
            roles.DeleteMany("1=1");
            userRoles.DeleteMany("1=1");
            permissions.DeleteMany("1=1");
            
            users.EnsureIndex("id");
            users.EnsureIndex("email");

            roles.EnsureIndex("id");
            userRoles.EnsureIndex("userId");
            permissions.EnsureIndex("permission");


            User user = new User();
            user.setId(1);
            user.setEmail("abc@plsar.net");
            user.setPassword("3b1a5b7b9b996e21e81ae1b12abacab5c463707ccb0206535889c815cde5f650");
            users.Insert(user);

            Role role = new Role();
            role.setId(1);
            role.setDescription("super-role");
            roles.Insert(role);

            UserRole userRole = new UserRole();
            userRole.setId(1);
            userRole.setUserId(1);
            userRole.setRoleId(1);
            userRoles.Insert(userRole);

            Permission permission = new Permission();
            permission.setId(1);
            permission.setUserId(1);
            permission.setPermission("users:maintenance:1");
            permissions.Insert(permission);

            var u = users.Query()
            .Where("$.Title LIKE '%1%' OR $.Description LIKE '%1%'")
            .ToArray();
            var bsonReader = db.Execute("select $ from users where email = 'abc@plsar.net'");
            ArrayList output = new ArrayList();
            while (bsonReader.Read())output.Add(bsonReader.Current);
            if(output.Count > 0){
                var userBson = output[0];
                User z = new User();
                Console.WriteLine(userBson);
            }

            Console.WriteLine("z: {0}", (users.Count()));

            ApplicationAttributes applicationAttributes = new ApplicationAttributes();
            applicationAttributes.getAttributes().Add("abc", "123");
            applicationAttributes.getAttributes().Add("db", "Ocean.db");

            SkylineRunnable skyline = new SkylineRunnable(4000);
            skyline.setNumberOfPartitions(30);
            skyline.setNumberOfRequestExecutors(70);

            PersistenceConfig persistenceConfig = new PersistenceConfig();

            ViewConfig viewConfig = new ViewConfig();
            viewConfig.setResourcesPath("Assets");
            viewConfig.setRenderingScheme(RenderingScheme.RELOAD_EACH_REQUEST);

            skyline.setSecurityAccessType(new AuthAccess().GetType());

            SecurityManager manager = new SecurityManager(new AuthAccess(new DataTransferObject(persistenceConfig)));
            manager.signin("abc@plsar.net", "effort.", new NetworkRequest(), new NetworkResponse());

            skyline.setApplicationAttributes(applicationAttributes);
            skyline.setPersistenceConfig(persistenceConfig);
            skyline.setViewConfig(viewConfig);

            skyline.start();
            return 0;
        }
    }
}

