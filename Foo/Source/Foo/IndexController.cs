using System;
using Skyline;

namespace Foo{
    
    // [NetworkController]
    public class IndexController{
        
        ApplicationAttributes applicationAttributes;
        
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