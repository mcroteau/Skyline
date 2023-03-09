using System;
using System.Collections.Generic;

using Zeus;
using Zeus.Model;
using Zeus.Security;
using Zeus.Implement;

namespace Zeus.Model {
    public class RouteAttributes {

        public RouteAttributes(){
            this.attributes = new Dictionary<string, string>();
            this.viewRenderers = new Dictionary<string, ViewRenderer>();
            this.routeEndpointHolder = new RouteEndpointHolder();
            this.sessionRegistry = new Dictionary<string, bool>();
        }

        PersistenceConfig persistenceConfig;

        Dictionary<String, String> attributes;
        Dictionary<String, ViewRenderer> viewRenderers;
        Dictionary<String, Boolean> sessionRegistry;
        RouteEndpointHolder routeEndpointHolder;

        String securityAccessKlass;
        SecurityManager securityManager;

        public PersistenceConfig getPersistenceConfig()
        {
            return this.persistenceConfig;
        }

        public void setPersistenceConfig(PersistenceConfig persistenceConfig)
        {
            this.persistenceConfig = persistenceConfig;
        }

        public Dictionary<String, String> getAttributes()
        {
            return this.attributes;
        }

        public void setAttributes(Dictionary<String, String> attributes)
        {
            this.attributes = attributes;
        }

        public Dictionary<String, ViewRenderer> getViewRenderers()
        {
            return this.viewRenderers;
        }

        public void setViewRenderers(Dictionary<String, ViewRenderer> viewRenderers)
        {
            this.viewRenderers = viewRenderers;
        }

        public Dictionary<String, bool> getSessionRegistry()
        {
            return this.sessionRegistry;
        }

        public void setSessionRegistry(Dictionary<String, bool> sessionRegistry)
        {
            this.sessionRegistry = sessionRegistry;
        }

        public RouteEndpointHolder getRouteEndpointHolder()
        {
            return this.routeEndpointHolder;
        }

        public void setRouteEndpointHolder(RouteEndpointHolder routeEndpointHolder)
        {
            this.routeEndpointHolder = routeEndpointHolder;
        }

        public String getSecurityAccessKlass()
        {
            return this.securityAccessKlass;
        }

        public void setSecurityAccessKlass(String securityAccessKlass)
        {
            this.securityAccessKlass = securityAccessKlass;
        }

        public SecurityManager getSecurityManager()
        {
            return this.securityManager;
        }

        public void setSecurityManager(SecurityManager securityManager)
        {
            this.securityManager = securityManager;
        }


    }
}