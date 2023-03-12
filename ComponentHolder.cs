using System;
using System.Collections.Generic;

namespace Skyline {
    public class ComponentsHolder{
        Type serverStartup;
        Type routeRegistration;
        Dictionary<String, String> repositories;

        public Type getServerStartup()
        {
            return this.serverStartup;
        }

        public void setServerStartup(Type serverStartup)
        {
            this.serverStartup = serverStartup;
        }

        public Type getRouteRegistration()
        {
            return this.routeRegistration;
        }

        public void setRouteRegistration(Type routeRegistration)
        {
            this.routeRegistration = routeRegistration;
        }

        public Dictionary<String, String>  getRepositories()
        {
            return this.repositories;
        }

        public void setRepositories(Dictionary<String, String> repositories)
        {
            this.repositories = repositories;
        }


    }
}