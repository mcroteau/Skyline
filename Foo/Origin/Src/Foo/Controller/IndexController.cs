using System;
using System.Collections;

using Skyline;
using Skyline.Model;
using Skyline.Annotation;
using Skyline.Security;

using Foo.Repo;

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
        
        [Layout(file="Default.asp")]
        [Get(route="/")]
        public String index(NetworkRequest req, NetworkResponse resp, SecurityManager manager, FlashMessage flashMessage, ViewCache cache){
            cache.set("message", "");

            ArrayList items = new ArrayList();
            items.Add("Cinco DeMayos");
            items.Add("Burgers n' Fries");
            items.Add("Canolis");
            cache.set("items", items);

            return "Index.asp";
        }

        [Layout(file="Default.asp")]
        [Get(route="/secured")]
        public String sec(NetworkRequest req, NetworkResponse resp, SecurityManager manager){
            Console.WriteLine("\n\n***************************\n\n");
            if(!manager.isAuthenticated(req)){
                return "redirect:/";
            }
            return "Secured.asp";
        }

    }

}