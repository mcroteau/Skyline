using System;
using System.Collections;
using System.Collections.Generic;

namespace Skyline.Model{

    public class MethodComponents {
        public MethodComponents(){
            this.routeMethodAttributeVariablesList = new ArrayList();
            this.routeMethodAttributesList = new ArrayList();
            this.routeMethodAttributes = new Dictionary<String, MethodAttribute>();
        }

        ArrayList routeMethodAttributesList;
        ArrayList routeMethodAttributeVariablesList;
        Dictionary<String, MethodAttribute> routeMethodAttributes;

        public ArrayList getRouteMethodAttributesList() {
            return this.routeMethodAttributesList;
        }

        public void setRouteMethodAttributesList(ArrayList routeMethodAttributesList) {
            this.routeMethodAttributesList = routeMethodAttributesList;
        }

        public ArrayList getRouteMethodAttributeVariablesList() {
            return this.routeMethodAttributeVariablesList;
        }

        public void setRouteMethodAttributeVariablesList(ArrayList routeMethodAttributeVariablesList) {
            this.routeMethodAttributeVariablesList = routeMethodAttributeVariablesList;
        }

        public Dictionary<String, MethodAttribute> getRouteMethodAttributes() {
            return this.routeMethodAttributes;
        }

        public void setRouteMethodAttributes(Dictionary<String, MethodAttribute> routeMethodAttributes) {
            this.routeMethodAttributes = routeMethodAttributes;
        }

    }

}