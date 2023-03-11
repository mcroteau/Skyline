using System;

namespace AeonFlux{

    public class PersistenceConfig {
        public PersistenceConfig(){
            this.debug = false;
        }
        Boolean debug;
        String environment;
        String modelsPath;

        public Boolean getDebug()
        {
            return this.debug;
        }

        public void setDebug(Boolean debug)
        {
            this.debug = debug;
        }

        public String getEnvironment()
        {
            return this.environment;
        }

        public void setEnvironment(String environment)
        {
            this.environment = environment;
        }

        public String getModelsPath()
        {
            return this.modelsPath;
        }

        public void setModelsPath(String modelsPath)
        {
            this.modelsPath = modelsPath;
        }



    }
}