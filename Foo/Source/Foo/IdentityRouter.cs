using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Foo{


    [NetworkController]
    public class IdentityRouter{

        public IdentityRouter(){}

        ApplicationAttributes applicationAttributes;
        public IdentityRouter(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Layout(file="/Pages/Default.asp")]        
        [Get(route="/signin")]
        public String signin(){
            return "/Pages/Signin.asp";
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