using System;
using System.Collections;

using Skyline;
using Skyline.Model;
using Skyline.Annotation;
using Skyline.Security;

using Foo.Repo;

using Newtonsoft.Json;

namespace Foo.Controller{
    
    [NetworkController]
    public class IndexController{
        
        ApplicationAttributes applicationAttributes;

        [Bind]
        public PersonRepo personRepo;

        public IndexController(){}

        public IndexController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
        
        [Text]
        [Get(route="/status")]
        public String index(){
            return "hi";
        }
    
        [Layout(file="views/Default.asp")]
        [Get(route="/")]
        public String index(NetworkRequest req, NetworkResponse resp, SecurityManager manager, FlashMessage flashMessage, ViewCache cache){
            cache.set("message", "");

            ArrayList items = new ArrayList();
            items.Add("Cinco DeMayos");
            items.Add("Dulche & Gabanas");
            items.Add("I He^rt Radio");
            cache.set("items", items);

            return "views/Index.asp";
        }

        [Layout(file="views/Default.asp")]
        [Get(route="/secured")]
        public String sec(NetworkRequest req, NetworkResponse resp, SecurityManager manager){
            Console.WriteLine("\n\n***************************\n\n");
            if(!manager.isAuthenticated(req)){
                return "redirect:/";
            }
            return "views/Secured.asp";
        }

    }

}