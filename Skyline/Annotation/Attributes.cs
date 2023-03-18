using System;

namespace Skyline.Annotation{
    
    [System.AttributeUsage(System.AttributeTargets.Method)] 
    public class Attributes : Attribute{
        public String headline;
        public String keywords;
        public String description;

        public String getHeadline() {
            return this.headline;
        }

        public void setHeadline(String headline) {
            this.headline = headline;
        }

        public String getKeywords() {
            return this.keywords;
        }

        public void setKeywords(String keywords) {
            this.keywords = keywords;
        }

        public String getDescription() {
            return this.description;
        }

        public void setDescription(String description) {
            this.description = description;
        }
    }
}
