using System;
using System.Collections.Generic;

namespace Skyline {
    public class ComponentsHolder{

        public ComponentsHolder(){
            this.repositories = new Dictionary<String, Object>();
        }

        Object serverStartup;
        Object routeRegistration;
        Dictionary<String, Object> repositories;

        public Object getServerStartup() {
            return this.serverStartup;
        }

        public void setServerStartup(Object serverStartup) {
            this.serverStartup = serverStartup;
        }

        public Object getRouteRegistration() {
            return this.routeRegistration;
        }

        public void setRouteRegistration(Object routeRegistration) {
            this.routeRegistration = routeRegistration;
        }

        public Dictionary<String, Object> getRepositories() {
            return this.repositories;
        }

        public void setRepositories(Dictionary<String, Object> repositories) {
            this.repositories = repositories;
        }
    }
}