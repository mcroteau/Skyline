using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

using Persistence.Model;
using Persistence.Repo;

namespace Persistence.Controller{

    [NetworkController]
    public class IdentityController{

        [Bind]
        public UserRepo userRepo;

        [Bind]
        public RoleRepo roleRepo;

        [Bind]
        public PermissionRepo permissionRepo;


        public IdentityController(){}

        ApplicationAttributes applicationAttributes;
        
        public IdentityController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Layout(file="pages/Default.ux")]        
        [Get(route="/signin")]
        public String signin(NetworkRequest req, ViewCache cache){           
            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);
            return "pages/Signin.ux";
        }

        [Layout(file="pages/Default.ux")]        
        [Get(route="/signup")]
        public String signup(NetworkRequest req, ViewCache cache){           
            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);
            return "pages/Signup.ux";
        }

        [Post(route="/signin")]
        public String signin(NetworkRequest req, 
                            NetworkResponse resp, 
                            SecurityManager manager, 
                            ViewCache cache){            
            String email = req.getValue("email");
            String password = req.getValue("password");
            
            manager.signout(req, resp);

            if(manager.signin(email, password, req, resp)){
                return "redirect:/secured";
            }
            cache.set("message", "please try again.");
            return "redirect:/signin";
        }

        [Get(route="/signout")]
        public String signout(NetworkRequest req, NetworkResponse resp, SecurityManager manager){
            manager.signout(req, resp);
            return "redirect:/signin";
        }

        [Post(route="/register")]
        public String register(NetworkRequest req, NetworkResponse resp, SecurityManager manager, ViewCache cache){            
            String email = req.getValue("email");
            String password = req.getValue("password");
            
            User user = new User();
            user.setEmail(email);
            user.setPassword(password);
            long id = userRepo.save(user);

            UserRole userRole = new UserRole();
            userRole.setUserId(Convert.ToInt32(id));
            userRole.setRoleId(1);
            roleRepo.save(userRole);

            Permission permission = new Permission();
            permission.setUserId(Convert.ToInt32(id));
            permission.setPermission("users:maintenance:" + id);
            permissionRepo.save(permission);

            cache.set("message", "successfully registered.");
            return "redirect:/signin";
        }
    }
}