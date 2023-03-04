using System;
using System.Text;
using System.Collections.Generic;

namespace Zeus {
        
    public class DataPartial {
        int idx;
        string guid;
        string entry;
        string field;
        bool spec;
        bool iterable;
        bool specRequired;
        bool withinSpec;
        bool withinIterable;
        bool endIterable;
        bool endSpec;
        bool setVar;
        List<object> mojos;
        List<ObjectComponent> components;
        List<DataPartial> specPartials;
        Dictionary<string, DataPartial> specPartialsMap;//todo:remove specpartials list rename

        public DataPartial(){
            this.guid = System.Guid.NewGuid().ToString();
            this.field = "";
            this.mojos = new List<object>();
            this.components = new List<ObjectComponent>();
            this.specPartials = new List<DataPartial>();
            this.specPartialsMap = new Dictionary<string, DataPartial>();
        }

    }
}