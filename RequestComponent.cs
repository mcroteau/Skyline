using System;

namespace Zeus{
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

        public boolean isHasFiles()
        {
            return this.hasFiles;
        }

        public void setHasFiles(boolean hasFiles)
        {
            this.hasFiles = hasFiles;
        }

    }
}