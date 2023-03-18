using System;

using Skyline;
using Skyline.Model;
using Skyline.Annotation;

namespace Foo{
    
    [NetworkController]
    public class IndexController{
        
        ApplicationAttributes applicationAttributes;

        public IndexController(){}

        public IndexController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Text]
        [Get(route="/")]
        public String index(ViewCache viewCache, NetworkRequest req, NetworkResponse resp){
            Console.WriteLine("idx.");
            return "hi";
        }
    }

}