using System;
using System.Collections.Generic;

namespace Zeus{
    public class IterableResult {
        String field;
        String key;
        List<Object> mojos;

        public String getField()
        {
            return this.field;
        }

        public void setField(String field)
        {
            this.field = field;
        }

        public String getKey()
        {
            return this.key;
        }

        public void setKey(String key)
        {
            this.key = key;
        }

        public List<Object> getMojos()
        {
            return this.mojos;
        }

        public void setMojos(List<Object> mojos)
        {
            this.mojos = mojos;
        }
    }
}