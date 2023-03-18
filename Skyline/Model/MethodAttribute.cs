using System;

namespace Skyline.Model{
    public class MethodAttribute {
        String description;
        Object attribute;

        public String getDescription() {
            return this.description;
        }

        public void setDescription(String description) {
            this.description = description;
        }

        public Object getAttribute() {
            return this.attribute;
        }

        public void setAttribute(Object attribute) {
            this.attribute = attribute;
        }
    }
}