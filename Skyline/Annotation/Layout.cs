using System;

namespace Skyline.Annotation{
    
    [System.AttributeUsage(System.AttributeTargets.Method)] 
    public class Layout : Attribute{
        public String file;

        public String getFile() {
            return this.file;
        }

        public void setFile(String file) {
            this.file = file;
        }
    }
}