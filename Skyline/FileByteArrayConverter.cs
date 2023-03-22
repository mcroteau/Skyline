using System;
using System.IO;

namespace Skyline{
    public class FileByteArrayConverter{
        String file;
        ViewConfig viewConfig;

        public byte[] convert(){
            String filePath = "Webapp" + Path.DirectorySeparatorChar.ToString()
                + Path.DirectorySeparatorChar.ToString() + file;

            Console.WriteLine("filePath~" + filePath);
            
            FileStream fileInputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileInputStream);
            long byteLength = new System.IO.FileInfo(filePath).Length;
            byte[] fileContent = binaryReader.ReadBytes((Int32)byteLength);
            
            fileInputStream.Close();
            fileInputStream.Dispose();
            binaryReader.Close();

            return fileContent;
        }

        public String getFile() {
            return this.file;
        }

        public void setFile(String file) {
            this.file = file;
        }

        public ViewConfig getViewConfig() {
            return this.viewConfig;
        }

        public void setViewConfig(ViewConfig viewConfig) {
            this.viewConfig = viewConfig;
        }
    }
}