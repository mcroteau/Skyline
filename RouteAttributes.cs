using System;
using System.Collections.Generic;

namespace Zeus{
    public class RouteAttributes {

        public RouteAttributes(){
            this.attributes = new ConcurrentHashMap<>();
            this.routeEndpointHolder = new RouteEndpointHolder();
            this.sessionRegistry = new ConcurrentHashMap<>();
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

        public String> getAttributes()
        {
            return this.attributes;
        }

        public void setAttributes(String> attributes)
        {
            this.attributes = attributes;
        }

        public ViewRenderer> getViewRenderers()
        {
            return this.viewRenderers;
        }

        public void setViewRenderers(ViewRenderer> viewRenderers)
        {
            this.viewRenderers = viewRenderers;
        }

        public Boolean> getSessionRegistry()
        {
            return this.sessionRegistry;
        }

        public void setSessionRegistry(Boolean> sessionRegistry)
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