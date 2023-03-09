using System;

namespace Zeus.Model {
    public class RequestComponent {
        String name;
        String value;
        Boolean hasFiles;

        public String getName()
        {
            return this.name;
        }

        public void setName(String name)
        {
            this.name = name;
        }

        public String getValue()
        {
            return this.value;
        }

        public void setValue(String value)
        {
            this.value = value;
        }

        public Boolean isHasFiles()
        {
            return this.hasFiles;
        }

        public void setHasFiles(Boolean hasFiles)
        {
            this.hasFiles = hasFiles;
        }

    }
}