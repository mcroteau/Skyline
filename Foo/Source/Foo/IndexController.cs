using System;
using System.Collections;

using Skyline;
using Skyline.Model;
using Skyline.Annotation;

using Foo.Repo;

namespace Foo{
    
    [NetworkController]
    public class IndexController{
        
        ApplicationAttributes applicationAttributes;

        [Bind]
        public PersonRepo personRepo;

        public IndexController(){}

        public IndexController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
        
        [Layout(file="/Pages/Default.asp")]
        [Get(route="/")]
        public String index(ViewCache cache, NetworkRequest req, NetworkResponse resp){
            cache.set("message", "here...");

            ArrayList items = new ArrayList();
            items.Add("Cinco DeMayos");
            items.Add("Burgers n' Fries");
            items.Add("Canolis");
            cache.set("items", items);

            return "/Pages/Index.asp";
        }

        [Text]
        [Get(route="/{id}")]
        public String index([Variable]Int128 id, ViewCache viewCache, NetworkRequest req, NetworkResponse resp){
            Console.WriteLine("." + id);
            return "hi";
        }

        [Text]
        [Get(route="/{stateid}/{evaluate}/{id}")]
        public String index([Variable]Int32 stateid, [Variable]Boolean evaluate, [Variable]Int32 id, ViewCache viewCache, NetworkRequest req, NetworkResponse resp){
            Console.WriteLine(stateid + ":" + evaluate + ":" + id);
            return "hi";
        }

    }

}