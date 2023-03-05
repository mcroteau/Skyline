using System;
using System.Int32;
using System.String;

namespace Zeus{
    public class RouteAttribute {

        String qualifiedName;
        String typeKlass;
        Int32 routePosition;
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

        public Int32 getRoutePosition()
        {
            return this.routePosition;
        }

        public void setRoutePosition(Int32 routePosition)
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