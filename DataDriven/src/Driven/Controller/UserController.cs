using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

using Driven.Model;
using Driven.Repo;

namespace Driven.Controller{

    [NetworkController]
    public class UserController{

        [Bind]
        UserRepo userRepo;

        ApplicationAttributes applicationAttributes;
        
        public UserController(){}

        public UserController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Layout(file="views/Default.asp")]        
        [Get(route="/users")]
        public String index(NetworkRequest req, ViewCache cache){

            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);

            return "views/Users/List.asp";
        }

        [Layout(file="views/Default.asp")]        
        [Get(route="/users/create")]
        public String create(NetworkRequest req, ViewCache cache){

            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);

            return "views/Users/Create.asp";
        }

        [Post(route="/users/save")]
        public String save(NetworkRequest req, NetworkResponse resp, SecurityManager manager, ViewCache cache){            
            String email = req.getValue("email");
            String password = req.getValue("password");
        
            cache.set("message", "");
            if(!manager.isAuthenticated(req)){
                cache.set("message", "authorization required.");
                return "redirect:/signin";
            }

            if(!manager.hasRole("super-role", req)){
                cache.set("message", "authorization required.");
                return "redirect:/";
            }
            
            User user = new User();
            user.setEmail(email);
            user.setPassword(password);
            long id = userRepo.save(user);

            cache.set("message", "success.");
            return "redirect/users/edit/" + id;
        }
        
        [Get(route="/users/edit/{id}")]
        public String edit(NetworkRequest req, 
                            NetworkResponse resp, 
                            SecurityManager manager, 
                            ViewCache cache, 
                            [Variable] Int32 id){
                                            
            cache.set("message", "");
            if(!manager.isAuthenticated(req)){
                cache.set("message", "authorization required.");
                return "redirect:/signin";
            }

            String permission = "users:maintentance:" + id;
            if(!manager.hasRole("super-role", req) && 
                    !manager.hasPermission(permission, req)){
                cache.set("message", "authorization required.");
                return "redirect:/";
            }
            
            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);

            User user = userRepo.getId(id);
            cache.set("user", user);

            return "redirect/users";
        }

        [Post(route="/users/update/{id}")]
        public String update(NetworkRequest req, 
                            NetworkResponse resp, 
                            SecurityManager manager, 
                            ViewCache cache, 
                            [Variable] Int32 id){
                                            
            cache.set("message", "");
            if(!manager.isAuthenticated(req)){
                cache.set("message", "authorization required.");
                return "redirect:/signin";
            }

            String permission = "users:maintentance:" + id;
            if(!manager.hasRole("super-role", req) && 
                    !manager.hasPermission(permission, req)){
                cache.set("message", "authorization required.");
                return "redirect:/";
            }
            
            String email = req.getValue("email");
            String password = req.getValue("password");

            User user = userRepo.getId(id);
            user.setEmail(email);
            user.setPassword(password);

            userRepo.update(user);

            cache.set("message", "success.");
            return "redirect/users/edit/" + id;
        }

        [Post(route="/users/delete/{id}")]
        public String delete(NetworkRequest req, 
                            NetworkResponse resp, 
                            SecurityManager manager, 
                            ViewCache cache, 
                            [Variable] Int32 id){
                                            
            cache.set("message", "");
            if(!manager.isAuthenticated(req)){
                cache.set("message", "authorization required.");
                return "redirect:/signin";
            }

            String permission = "users:maintentance:" + id;
            if(!manager.hasRole("super-role", req) && 
                    !manager.hasPermission(permission, req)){
                cache.set("message", "authorization required.");
                return "redirect:/";
            }
            
            userRepo.delete(id);
            
            cache.set("message", "success.");
            return "redirect/users";
        }
    }
}