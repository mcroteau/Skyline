using System;
using System.IO;

namespace Skyline{
    public class ResourceFileConverter{
        String file;

        public byte[] convert(){

            String filePath = Directory.GetCurrentDirectory() + file;
            
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

    }
}