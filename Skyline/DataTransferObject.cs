using System;

using Skyline.Model;

namespace Skyline{
    public class DataTransferObject{
        ApplicationAttributes applicationAttributes;
        public DataTransferObject(PersistenceConfig persistenceConfig){
            
        }   
        public ApplicationAttributes getApplicationAttributes() {
            return this.applicationAttributes;
        }

        public void setApplicationAttributes(ApplicationAttributes applicationAttributes) {
            this.applicationAttributes = applicationAttributes;
        }
    }
}