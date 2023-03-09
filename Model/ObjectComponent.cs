using System;

namespace Tiger.Model {
    public class ObjectComponent {
        string activeField;
        object instance;

        public string getActiveField()
        {
            return this.activeField;
        }

        public void setActiveField(string activeField)
        {
            this.activeField = activeField;
        }

        public object getInstance()
        {
            return this.instance;
        }

        public void setInstance(object instance)
        {
            this.instance = instance;
        }


        public string toString(){
            return this.activeField + ":" + this.instance;
        }
    }
}
