using System;

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

        [Text]
        [Get(route="/")]
        public String index(ViewCache viewCache, NetworkRequest req, NetworkResponse resp){
            Console.WriteLine("idx.");
            Console.WriteLine(applicationAttributes.getAttributes()["abc"]);
            personRepo.print();
            return "hi";
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