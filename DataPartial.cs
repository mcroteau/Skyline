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
        Dictionary<string, DataPartial> specPartialsMap;


        public int getIdx()
        {
            return this.idx;
        }

        public void setIdx(int idx)
        {
            this.idx = idx;
        }

        public string getGuid()
        {
            return this.guid;
        }

        public void setGuid(string guid)
        {
            this.guid = guid;
        }

        public string getEntry()
        {
            return this.entry;
        }

        public void setEntry(string entry)
        {
            this.entry = entry;
        }

        public string getField()
        {
            return this.field;
        }

        public void setField(string field)
        {
            this.field = field;
        }

        public bool isSpec()
        {
            return this.spec;
        }

        public void setSpec(bool spec)
        {
            this.spec = spec;
        }

        public bool isIterable()
        {
            return this.iterable;
        }

        public void setIterable(bool iterable)
        {
            this.iterable = iterable;
        }

        public bool isSpecRequired()
        {
            return this.specRequired;
        }

        public void setSpecRequired(bool specRequired)
        {
            this.specRequired = specRequired;
        }

        public bool isWithinSpec()
        {
            return this.withinSpec;
        }

        public void setWithinSpec(bool withinSpec)
        {
            this.withinSpec = withinSpec;
        }

        public bool isWithinIterable()
        {
            return this.withinIterable;
        }

        public void setWithinIterable(bool withinIterable)
        {
            this.withinIterable = withinIterable;
        }

        public bool isEndIterable()
        {
            return this.endIterable;
        }

        public void setEndIterable(bool endIterable)
        {
            this.endIterable = endIterable;
        }

        public bool isEndSpec()
        {
            return this.endSpec;
        }

        public void setEndSpec(bool endSpec)
        {
            this.endSpec = endSpec;
        }

        public bool isSetVar()
        {
            return this.setVar;
        }

        public void setSetVar(bool setVar)
        {
            this.setVar = setVar;
        }
//todo:remove specpartials list rename
        public List<object> getMojos()
        {
            return this.mojos;
        }

        public void setMojos(List<object> mojos)
        {
            this.mojos = mojos;
        }
        public List<ObjectComponent> getComponents()
        {
            return this.components;
        }

        public void setComponents(List<ObjectComponent> components)
        {
            this.components = components;
        }
        public List<DataPartial> getSpecPartials()
        {
            return this.specPartials;
        }

        public void setSpecPartials(List<DataPartial> specPartials)
        {
            this.specPartials = specPartials;
        }

        public Dictionary<string, DataPartial> getSpecPartialsMap()
        {
            return this.specPartialsMap;
        }

        public void setSpecPartialsMap(Dictionary<string, DataPartial> specPartialsMap)
        {
            this.specPartialsMap = specPartialsMap;
        }

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