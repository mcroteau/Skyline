using System;
using Skyline;
using Skyline.Model;

namespace Foo{
    
    // [NetworkController]
    public class IndexController{
        
        ApplicationAttributes applicationAttributes;

        public IndexController(){}

        public IndexController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        // [Text]
        // [Get(route="/")]
        // public String index(){
        //     return "hi";
        // }
    }

}