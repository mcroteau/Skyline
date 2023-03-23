using System;

namespace Skyline.Model {
    public class RequestComponent {
        String name;
        String value;
        Boolean hasFiles;

        List<FileComponent> fileComponents;
        List<String> values;

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

        public List<FileComponent> getFileComponents() {
            return this.fileComponents;
        }

        public void setFileComponents(List<FileComponent> fileComponents) {
            this.fileComponents = fileComponents;
        }

        public List<String> getValues() {
            return this.values;
        }

        public void setValues(List<String> values) {
            this.values = values;
        }
    }
}