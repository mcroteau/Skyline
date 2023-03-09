using System;

namespace Tiger.Model {
    public class RouteAttribute {

        String qualifiedName;
        String typeKlass;
        int routePosition;
        Boolean routeVariable;

        public String getQualifiedName()
        {
            return this.qualifiedName;
        }

        public void setQualifiedName(String qualifiedName)
        {
            this.qualifiedName = qualifiedName;
        }

        public String getTypeKlass()
        {
            return this.typeKlass;
        }

        public void setTypeKlass(String typeKlass)
        {
            this.typeKlass = typeKlass;
        }

        public int getRoutePosition()
        {
            return this.routePosition;
        }

        public void setRoutePosition(int routePosition)
        {
            this.routePosition = routePosition;
        }

        public Boolean getRouteVariable()
        {
            return this.routeVariable;
        }

        public void setRouteVariable(Boolean routeVariable)
        {
            this.routeVariable = routeVariable;
        }
    }
}