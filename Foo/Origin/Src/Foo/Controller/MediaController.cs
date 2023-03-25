using System;

using Skyline;
using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Foo.Controller{
    
    [NetworkController]
    public class MediaController{

        ApplicationAttributes applicationAttributes;

        public MediaController(){}

        public MediaController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }

        [Get(route="/upload")]
        public String upload(){
            return "Views/Media.asp";
        }
        
    }
}