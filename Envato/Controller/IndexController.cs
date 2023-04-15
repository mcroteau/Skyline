using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Controller{

    [NetworkController]
    public class IndexController{

        public IndexController(){}

        ApplicationAttributes applicationAttributes;
        
        public IndexController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
    
        [Layout(file="Pages/Default.ux")]        
        [Get(route="/")]
        public String index(NetworkRequest req, ViewCache cache){

            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);

            return "Pages/Index.ux";
        }

    }

}