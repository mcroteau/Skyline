using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using AeonFlux;
using AeonFlux.Specs;

namespace AeonFlux{
    public class AeonHelper{

        public String getGuid(int n) {
            Guid g = Guid.NewGuid();
            return g.ToString();
        }

        public int getTime(int days){
            String dateTime = DateTime.Now.AddMinutes(days * 24 * 60).ToString("yyyyMMddHHmmssSSS");
            return Int32.Parse(dateTime);
        }

        public String getSecurityAttribute(Dictionary<String, String> headers, String id){
            String value = null;
            String cookies = headers.GetValueOrDefault("cookie", "");
            if(cookies != null) {
                String[] bits = cookies.Split(";");
                foreach(String completes in bits) {
                    String[] parts = completes.Split("=");
                    String key = parts[0].Trim();
                    if (parts.Length == 2) {
                        if (key.Equals(id)) {
                            value = parts[1].Trim();
                        }
                    }
                }
            }
            return value;
        }

        public String getRedirect(String uri){
            String[] redirectBits = uri.Split(":");
            if(redirectBits.Length > 1)return redirectBits[1];
            return null;
        }

        public byte[] getViewFileCopy(String viewKey, Dictionary<String, byte[]> viewBytesMap) {
            if(viewBytesMap.ContainsKey(viewKey)){
                byte[] byteArray = viewBytesMap.GetValueOrDefault(viewKey, new byte[]{});
                byte[] byteArrayCopy = new byte[byteArray.GetLength(0)];
                int indx = 0;
                foreach(byte b in byteArray){
                    byteArrayCopy[indx] = b;
                    indx++;
                }
                return byteArrayCopy;
            }
            return new byte[]{};
        }

        public Dictionary<String, byte[]> getViewBytesMap(ViewConfig viewConfig) {
            Dictionary<String, byte[]> viewFilesBytesMap = new Dictionary<String, byte[]>();

            if(Directory.Exists(viewConfig.getViewsPath())){
                String[] viewFiles = Directory.GetFiles(viewConfig.getViewsPath());
                getFileBytesMap(viewFiles, viewConfig, viewFilesBytesMap);
            }

            if(Directory.Exists(viewConfig.getResourcesPath())) {
                String[] resourceFiles = Directory.GetFiles(viewConfig.getResourcesPath());
                getFileBytesMap(resourceFiles, viewConfig, viewFilesBytesMap);
            }

            return viewFilesBytesMap;
        }

        Dictionary<String, byte[]> getFileBytesMap(String[] viewFiles, ViewConfig viewConfig, Dictionary<String, byte[]> viewFilesBytesMap){
            foreach(String viewFile in viewFiles) {
                
                if(Directory.Exists(viewFile)){
                    String[] directoryFiles = Directory.GetFiles(viewFile);
                    getFileBytesMap(directoryFiles, viewConfig, viewFilesBytesMap);
                    continue;
                }

                try {

                    Stream source = File.OpenRead(viewFile); 
                    MemoryStream dest = new MemoryStream();
                    byte[] buffer = new byte[1024 * 3];
                    int bytesRead;
                    while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
                        dest.Write(buffer, 0, bytesRead);
                    }
                
                    byte[] viewFileBytes = dest.ToArray();
                    String viewKey = viewFile.ToString().Replace(viewConfig.getViewsPath(), "");
                    viewFilesBytesMap.Add(viewKey, viewFileBytes);

                } catch (IOException ex) {
                    Console.WriteLine(ex.Message);
                }
            }
            return viewFilesBytesMap;
        }
    }
}