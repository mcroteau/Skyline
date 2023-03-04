using System;

namespace Zeus {
    public class ObjectComponent {
        string activeField;
        object instance;

        public string toString(){
            return this.activeField + ":" + this.instance;
        }
    }
}
