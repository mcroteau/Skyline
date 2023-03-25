
using Skyline.Model;
using Skyline.Annotation;
using Skyline.Implement;


namespace Foo{
    
    [ServerStartup]
    public class Startup : ServerListener {
        public void Start(){
            Console.WriteLine("starting up.");
        }
    }
}