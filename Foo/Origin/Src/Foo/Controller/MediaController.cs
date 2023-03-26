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
            return "Media.asp";
        }
        
        [Post(route="/upload")]
        public String upload(NetworkRequest req, ViewCache cache){
            RequestComponent requestComponent = req.getRequestComponents()["media"];
            FileComponent fileComponent = requestComponent.getFileComponents()[0];
            Console.WriteLine("file=" + fileComponent.getFileName());
            cache.set("message", "success. " + fileComponent.getFileName());
            return "redirect:/upload";
        }
    }
}