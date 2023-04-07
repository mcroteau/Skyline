using System;
using Skyline.Model;

namespace Skyline{
    public class RouteAttributesResolver{
        PropertiesConfig propertiesConfig;

        public RouteAttributes resolve(){
            RouteAttributes routeAttributes = new RouteAttributes();
            String propertiesPath = Directory.GetCurrentDirectory() + 
                Path.DirectorySeparatorChar.ToString() + propertiesConfig.getPropertiesFile();

            if (!File.Exists(propertiesPath)) {
                return routeAttributes;
            }

            foreach(var row in File.ReadAllLines(propertiesPath)){
                if(!row.Equals("")){
                    String key = row.Split('=')[0];
                    String property = row.Split('=')[1];
                    routeAttributes.getAttributes().Add(key, property);
                }
            }

            return routeAttributes;
        }    
                
        public void setPropertiesConfig(PropertiesConfig propertiesConfig) {
            this.propertiesConfig = propertiesConfig;
        }
    }
}