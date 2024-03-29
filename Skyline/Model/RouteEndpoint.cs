using System;
using System.Reflection;
using System.Collections.Generic;

namespace Skyline.Model {
    public class RouteEndpoint {
        public RouteEndpoint(){
            this.routeAttributes = new Dictionary<String, RouteAttribute>();
        }

        String routePath;
        String regexRoutePath;
        String routeVerb;

        MethodInfo routeMethod;
        String klassReference;
        String klassAssembly;
        Boolean regex;

        Dictionary<String, RouteAttribute> routeAttributes;

        public String getRoutePath() {
            return this.routePath;
        }

        public void setRoutePath(String routePath) {
            this.routePath = routePath;
        }

        public String getRegexRoutePath() {
            return this.regexRoutePath;
        }

        public void setRegexRoutePath(String regexRoutePath) {
            this.regexRoutePath = regexRoutePath;
        }

        public String getRouteVerb() {
            return this.routeVerb;
        }

        public void setRouteVerb(String routeVerb) {
            this.routeVerb = routeVerb;
        }

        public MethodInfo getRouteMethod() {
            return this.routeMethod;
        }

        public void setRouteMethod(MethodInfo routeMethod) {
            this.routeMethod = routeMethod;
        }

        public String getKlassReference() {
            return this.klassReference;
        }

        public void setKlassReference(String klassReference) {
            this.klassReference = klassReference;
        }

        public String getKlassAssembly() {
            return this.klassAssembly;
        }

        public void setKlassAssembly(String klassAssembly) {
            this.klassAssembly = klassAssembly;
        }

        public Boolean getRegex() {
            return this.regex;
        }

        public void setRegex(Boolean regex) {
            this.regex = regex;
        }

        public Dictionary<String, RouteAttribute> getRouteAttributes() {
            return this.routeAttributes;
        }

        public void setRouteAttributes(Dictionary<String, RouteAttribute> routeAttributes) {
            this.routeAttributes = routeAttributes;
        }
    }
}