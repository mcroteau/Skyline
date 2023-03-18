using System;
using System.IO;

namespace Skyline{
    public class FileToByteArrayConverter{
        String fileName;
        ViewConfig viewConfig;

        public byte[] convert(){
            String filePath = "Webapp" + Path.DirectorySeparatorChar.ToString() + 
                        viewConfig.getResourcesPath() + Path.DirectorySeparatorChar.ToString() + fileName;
            
            FileStream fileInputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileInputStream);
            long byteLength = new System.IO.FileInfo(fileName).Length;
            byte[] fileContent = binaryReader.ReadBytes((Int32)byteLength);
            
            fileInputStream.Close();
            fileInputStream.Dispose();
            binaryReader.Close();

            return fileContent;
        }

        public String getFileName() {
            return this.fileName;
        }

        public void setFileName(String fileName) {
            this.fileName = fileName;
        }

        public ViewConfig getViewConfig() {
            return this.viewConfig;
        }

        public void setViewConfig(ViewConfig viewConfig) {
            this.viewConfig = viewConfig;
        }
    }
}