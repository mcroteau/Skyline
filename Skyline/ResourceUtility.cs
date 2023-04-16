using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Skyline;
using Skyline.Specs;
using Skyline.Model;

namespace Skyline{
    public class ResourceUtility{

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
            
            if(!viewKey.StartsWith("/"))viewKey = "/" + viewKey;
            
            if(viewBytesMap.ContainsKey(viewKey)){
                return viewBytesMap[viewKey];
            }
            return new byte[]{};
        }

        public ResourcesHolder getResourcesHolder(ViewConfig viewConfig) {
            ResourcesHolder resourcesHolder = new ResourcesHolder();

            if(Directory.Exists(viewConfig.getViewsPath())){
                String[] viewFiles = Directory.GetFiles(@viewConfig.getViewsPath());
                getFileBytesMap(viewFiles, viewConfig, resourcesHolder);

                String[] directories = Directory.GetDirectories(@viewConfig.getViewsPath(), "*", SearchOption.TopDirectoryOnly);
                getFileBytesMap(directories, viewConfig, resourcesHolder);
            }

            if(Directory.Exists(viewConfig.getResourcesPath())) {
                String[] resourceFiles = Directory.GetFiles(@viewConfig.getResourcesPath());
                getFileBytesMap(resourceFiles, viewConfig, resourcesHolder);
                
                String[] directories = Directory.GetDirectories(@viewConfig.getResourcesPath(), "*", SearchOption.TopDirectoryOnly);
                getFileBytesMap(directories, viewConfig, resourcesHolder);
            }

            return resourcesHolder;
        }

        ResourcesHolder getFileBytesMap(String[] viewFiles, ViewConfig viewConfig, ResourcesHolder resourcesHolder){
            foreach(String file in viewFiles) {

                if(Directory.Exists(file)) {
                    String[] files = Directory.GetFiles(file);
                    getFileBytesMap(files, viewConfig, resourcesHolder);
                }
                
                if(File.Exists(file)){

                    try {

                        byte[] bytes = File.ReadAllBytes(file); 
                        
                        String resourceKey = file;
                        if(!resourceKey.StartsWith("/"))resourceKey = "/" + resourceKey;
                    
                        resourcesHolder.getResources().Add(resourceKey, bytes);

                    } catch (IOException ex) {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            return resourcesHolder;
        }
    }
}