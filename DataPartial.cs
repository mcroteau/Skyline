using System;
using System.Text;
using System.Collections.Generic;

namespace Zeus {
        
    public class DataPartial {
        int idx; public int Idx { get; set; }
        string guid; public string Guid { get; set; }
        string entry; public string Entry { get; set; }
        string field; public string Field { get; set; }
        bool spec; public int MyProperty { get; set; }
        bool iterable; public int MyProperty { get; set; }
        bool specRequired; public int MyProperty { get; set; }
        bool withinSpec; public int MyProperty { get; set; }
        bool withinIterable; public int MyProperty { get; set; }
        bool endIterable; public int MyProperty { get; set; }
        bool endSpec; public int MyProperty { get; set; }
        bool setVar; public int MyProperty { get; set; }
        List<object> mojos; public int MyProperty { get; set; }
        List<ObjectComponent> components; public int MyProperty { get; set; }
        List<DataPartial> specPartials; public int MyProperty { get; set; }
        Dictionary<string, DataPartial> specPartialsMap; public Dictionary SpecPartialsMap { get; set; }//todo:remove specpartials list rename

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