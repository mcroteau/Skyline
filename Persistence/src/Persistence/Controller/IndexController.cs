using System;

using Skyline.Model;
using Skyline.Security;
using Skyline.Annotation;

namespace Persistence.Controller{

    [NetworkController]
    public class IndexController{

        public IndexController(){}

        ApplicationAttributes applicationAttributes;
        
        public IndexController(ApplicationAttributes applicationAttributes){
            this.applicationAttributes = applicationAttributes;
        }
    
        [Layout(file="pages/Default.asp")]        
        [Get(route="/")]
        public String index(NetworkRequest req, ViewCache cache){

            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);

            return "pages/Index.asp";
        }

        [Layout(file="pages/Default.asp")]        
        [Get(route="/secured")]
        public String secured(NetworkRequest req, ViewCache cache){

            String sessionuser = req.getUserCredential();
            cache.set("sessionuser", sessionuser);

            return "pages/Secured.asp";
        }

    }

}