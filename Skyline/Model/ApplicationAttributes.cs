using System;

namespace Skyline.Model{

    public class ApplicationAttributes{

        public ApplicationAttributes(ApplicationAttributes applicationAttributes){
            this.attributes = applicationAttributes.getAttributes();
        }

        public ApplicationAttributes(){
            this.attributes = new Dictionary<String, Object>();
        }
        
        Dictionary<String, Object> attributes;

        public Dictionary<String, Object> getAttributes() {
            return this.attributes;
        }

        public void setAttributes(Dictionary<String, Object> attributes) {
            this.attributes = attributes;
        }
    }
}
