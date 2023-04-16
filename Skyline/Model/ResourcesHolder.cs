using System;

namespace Skyline.Model{
    public class ResourcesHolder{
        Dictionary<String, byte[]> resources;

        public ResourcesHolder(){
            this.resources = new Dictionary<String, byte[]>();
        }

        public ResourcesHolder(ResourcesHolder resourcesHolder){
            this.resources = resourcesHolder.getResources();
        }

        public Dictionary<String, byte[]> getResources(){
            return this.resources;
        }

        public void setResources(Dictionary<String, byte[]> resources){
            this.resources = resources;
        }

    }
}