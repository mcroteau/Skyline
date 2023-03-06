using System;
using System.Collections.Generic;

namespace Zeus{
    public class ViewCache {
        Dictionary<string, object> cache;

        public void set(string key, object value){
            this.cache.put(key, value);
        }
        public object get(string key){
            if(this.cache.containsKey(key)){
                return this.cache.get(key);
            }
            return null;
        }
        public Dictionary<string, object> getCache() {
            return cache;
        }
        public void setCache(Dictionary<string, object> cache) {
            this.cache = cache;
        }
        public ViewCache(){
            this.cache = new Dictionary<string, object>();
        }
    }
}