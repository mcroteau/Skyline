using System;
using System.IO;

namespace Skyline{
    public class ViewFileConverter{
        String file;
        String fileDirectory;

        public byte[] convert(){
            String filePath = "Origin" + Path.DirectorySeparatorChar.ToString() + fileDirectory +
                Path.DirectorySeparatorChar.ToString() + file;

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

        public String getFileDirectory(){
            return this.fileDirectory;
        }

        public void setFileDirectory(String fileDirectory){
            this.fileDirectory = fileDirectory;
        }
    }
}