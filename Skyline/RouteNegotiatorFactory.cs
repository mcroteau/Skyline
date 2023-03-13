using System;
using System.Reflection;
using Skyline;
using Skyline.Model;


namespace Skyline{
    public class RouteNegotiatorFactory{
        Object securityAccessKlass;
        PersistenceConfig persistenceConfig;
        ResourceUtility resourceUtility;
        RouteAttributes routeAttributes;
        SecurityAttributes securityAttributes;

        public RouteEndpointNegotiator create() {

            RouteEndpointNegotiator routeEndpointNegotiator = new RouteEndpointNegotiator();

            try {

                String securedAttribute = "attribute";
                String securityElement = "s.k.y.l.i.n.e";
                this.securityAttributes = new SecurityAttributes(securityElement, securedAttribute);

                RouteEndpointsResolver routeEndpointsResolver = new RouteEndpointsResolver(stargzrResources);
                RouteEndpointHolder routeEndpointHolder = routeEndpointsResolver.resolve();
                routeAttributes.setRouteEndpointHolder(routeEndpointHolder);

                if (persistenceConfig != null) {
                    PersistenceConfig persistenceConfig = new PersistenceConfig();
                    persistenceConfig.setDriver(this.persistenceConfig.getDriver());
                    persistenceConfig.setUrl(this.persistenceConfig.getUrl());
                    persistenceConfig.setUser(this.persistenceConfig.getUser());
                    persistenceConfig.setConnections(this.persistenceConfig.getConnections());
                    persistenceConfig.setPassword(this.persistenceConfig.getPassword());
                    routeAttributes.setPersistenceConfig(this.persistenceConfig);
                }

                if (securityAccessKlass != null) {
                    Dao dao = new Dao(persistenceConfig);
                    SecurityAccess securityAccessInstance = (SecurityAccess) securityAccessKlass.getConstructor().newInstance();
                    Method setPersistence = securityAccessInstance.getClass().getMethod("setDao", Dao.class);
                    setPersistence.invoke(securityAccessInstance, dao);
                    SecurityManager securityManager = new SecurityManager(securityAccessInstance);
                    routeAttributes.setSecurityManager(securityManager);
                    routeAttributes.setSecurityAccess(securityAccessKlass);
                }

                ComponentAnnotationInspector componentAnnotationInspector = new ComponentAnnotationInspector(new ComponentsHolder());
                componentAnnotationInspector.inspect();
                ComponentsHolder componentsHolder = componentAnnotationInspector.getComponentsHolder();

                routeEndpointNegotiator.setSecurityAttributes(securityAttributes);
                routeEndpointNegotiator.setRouteAttributes(routeAttributes);
                routeEndpointNegotiator.setComponentsHolder(componentsHolder);

            } catch (IllegalAccessException e) {
                e.printStackTrace();
            } catch (InvocationTargetException e) {
                e.printStackTrace();
            } catch (InstantiationException e) {
                e.printStackTrace();
            } catch (NoSuchMethodException e) {
                e.printStackTrace();
            }

            return routeEndpointNegotiator;
        }

        public SecurityAttributes getSecurityAttributes() {
            return securityAttributes;
        }

        public void setSecurityAttributes(SecurityAttributes securityAttributes) {
            this.securityAttributes = securityAttributes;
        }

        public Object getSecurityAccessKlass() {
            return securityAccessKlass;
        }

        public void setSecurityAccessKlass(Object securityAccessKlass) {
            this.securityAccessKlass = securityAccessKlass;
        }

        public PersistenceConfig getPersistenceConfig() {
            return persistenceConfig;
        }

        public void setPersistenceConfig(PersistenceConfig persistenceConfig) {
            this.persistenceConfig = persistenceConfig;
        }

        public StargzrResources getServerResources() {
            return stargzrResources;
        }

        public void setServerResources(StargzrResources stargzrResources) {
            this.stargzrResources = stargzrResources;
        }

        public RouteAttributes getRouteAttributes() {
            return routeAttributes;
        }

        public void setRouteAttributes(RouteAttributes routeAttributes) {
            this.routeAttributes = routeAttributes;
        }
    }
}