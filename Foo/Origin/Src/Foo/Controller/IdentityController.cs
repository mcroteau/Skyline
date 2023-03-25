using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Foo.Controller{


    [NetworkController]
    public class IdentityController{

        public IdentityController(){}

        ApplicationAttributes applicationAttributes;
        
        public IdentityController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Layout(file="Default.asp")]        
        [Get(route="/signin")]
        public String signin(){
            return "Signin.asp";
        }

        [Post(route="/signin")]
        public String signin(NetworkRequest req, NetworkResponse resp, SecurityManager manager, ViewCache cache){
            Console.WriteLine("reqz:" + req.getRequestComponentsList().Count);
            foreach(RequestComponent requestComponent in req.getRequestComponentsList()){
                Console.WriteLine("req:" + requestComponent.getName() + ":" + requestComponent.getValue());
            }
            
            String email = req.getValue("email");
            String password = req.getValue("password");
            Console.WriteLine("password:'" + password + "'");
            
            cache.set("message", "");//java, c#// new instances// c# is kind of awesome!

            if(manager.signin(email, password, req, resp)){
                Console.WriteLine("email:" + email + " password:" + password + " resp:" + resp);
                return "redirect:/secured";
            }
            cache.set("message","fail.");
            return "redirect:/signin";
        }
    }
}