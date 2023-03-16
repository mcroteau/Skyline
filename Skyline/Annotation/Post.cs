using System;

namespace Skyline.Annotation{
    
    [System.AttributeUsage(System.AttributeTargets.Method)] 
    public class Post : Attribute{
        public String value;

        public String getValue() {
            return this.value;
        }

        public void setValue(String value) {
            this.value = value;
        }
    }
}   