using System;
using System.Collections;
using System.Collections.Generic;

namespace Zeus.Model {
    public class IterableResult {
        String field;
        String key;
        ArrayList mojos;

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

        public ArrayList getMojos()
        {
            return this.mojos;
        }

        public void setMojos(ArrayList mojos)
        {
            this.mojos = mojos;
        }
    }
}