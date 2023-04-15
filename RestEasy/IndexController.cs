
using Skyline;
using Skyline.Model;
using Skyline.Annotation;


[NetworkController]
public class IndexController{
    
    ApplicationAttributes applicationAttributes;

    public IndexController(){}

    public IndexController(ApplicationAttributes applicationAttributes){
        this.applicationAttributes = applicationAttributes;
    }

    [Text]
    [Get(route="/")]
    public String index(){
        return "Hi!";
    }
}