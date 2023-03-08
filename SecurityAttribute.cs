using System;

namespace Zeus{
    public class SecurityAttribute {

        public SecurityAttribute(String name, String value){
            this.name = name;
            this.value = value;
        }

        String name;
        String value;
        String expires;

        public String getName()
        {
            return this.name;
        }

        public void setName(String name)
        {
            this.name = name;
        }

        public String getValue()
        {
            return this.value;
        }

        public void setValue(String value)
        {
            this.value = value;
        }

        public String getExpires()
        {
            return this.expires;
        }

        public void setExpires(String expires)
        {
            this.expires = expires;
        }

    }
}
