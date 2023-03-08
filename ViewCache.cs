using System;
using System.Collections.Generic;

namespace Zeus{
    public class ViewCache {
        Dictionary<String, Object> cache;

        public void set(String key, Object value){
            this.cache.Add(key, value);
        }
        public Object get(String key){
            if(this.cache.ContainsKey(key)){
                return this.cache.GetValueOrDefault(key, null);
            }
            return null;
        }
        public Dictionary<String, Object> getCache() {
            return cache;
        }
        public void setCache(Dictionary<String, Object> cache) {
            this.cache = cache;
        }
        public ViewCache(){
            this.cache = new Dictionary<String, Object>();
        }
    }
}