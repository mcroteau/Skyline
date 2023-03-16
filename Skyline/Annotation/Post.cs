using System;

namespace Skyline.Annotation{
    
    [System.AttributeUsage(System.AttributeTargets.Method)] 
    public class Post : Attribute{
        public String route;

        public String getRoute() {
            return this.route;
        }

        public void setRoute(String route) {
            this.route = route;
        }
    }
}   
