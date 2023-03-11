using System;
using System.IO;

namespace AeonFlux;

public class AeonHelper{

    public String getGuid(int n) {
        String CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";
        StringBuilder guid = new StringBuilder();
        int divisor = n/4;
        Random rnd = new Random();
        for(int z = 0; z < n;  z++) {
            if( z % divisor == 0 && z > 0) {
                guid.append("-");
            }
            int index = (int) (rnd.nextFloat() * CHARS.length());
            guid.append(CHARS.charAt(index));
        }
        return guid.toString();
    }

    public String getDefaultGuid(int n) {
        String CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";
        StringBuilder guid = new StringBuilder();
        Random rnd = new Random();
        for(int z = 0; z < n;  z++) {
            int index = (int) (rnd.nextFloat() * CHARS.length());
            guid.append(CHARS.charAt(index));
        }
        return guid.toString();
    }

    public String getSecurityGuid(int n) {
        String CHARS = "0123456789abcdefghijklmnopqrstuvwxyz";
        StringBuilder guid = new StringBuilder();
        int divisor = n/4;
        Random rnd = new Random();
        for(int z = 0; z < n;  z++) {
            if( z % divisor == 0 && z > 0) {
                guid.append(".");
            }
            int index = (int) (rnd.nextFloat() * CHARS.length());
            guid.append(CHARS.charAt(index));
        }
        return guid.toString();
    }

    public Long getTime(int days){
        String dateTime = DateTime.Now.AddMinutes(days * 24 * 60).ToString("yyyyMMddHHmmssSSS");
        return Int32.Parse(dateTime);
    }

    public String getSecurityAttribute(Map<String, String> headers, String id){
        String value = null;
        String cookies = headers.get("cookie");
        if(cookies != null) {
            String[] bits = cookies.split(";");
            foreach(String completes in bits) {
                String[] parts = completes.split("=");
                String key = parts[0].trim();
                if (parts.length == 2) {
                    if (key.equals(id)) {
                        value = parts[1].trim();
                    }
                }
            }
        }
        return value;
    }

    public String getRedirect(String uri){
        String[] redirectBits = uri.split(":");
        if(redirectBits.length > 1)return redirectBits[1];
        return null;
    }

    public ByteArrayOutputStream getViewFileCopy(String viewKey, Dictionary<String, byte[]> viewBytesMap) {
        if(viewBytesMap.ContainsKey(viewKey)){
            byte[] byteArray = viewBytesMap.Get(viewKey);
            byte[] byteArrayCopy = new byte[byteArray.GetLength()];
            int indx = 0;
            foreach(byte b in byteArray){
                byteArrayCopy[indx] = b;
                indx++;
            }
        }
        return byteArrayCopy;
    }

    public Dictionary<String, byte[]> getViewBytesMap(ViewConfig viewConfig) {
        Dictionary<String, byte[]> viewFilesBytesMap = new Dictionary<String, byte[]>();

        if(Directory.Exists(viewConfig.getViewsPath())){
            File[] viewFiles = Directory.GetFiles(viewConfig.getViewsPath());
            getFileBytesMap(viewFiles, viewConfig, viewFilesBytesMap);
        }

        if(resourcesDirectory.isDirectory(viewConfig.getResourcesPath())) {
            File[] resourceFiles = Directory.GetFiles(viewConfig.getResourcesPath());
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

                Stream dest = new Stream();
                Stream source = File.OpenRead(viewFile); 
                byte[] buffer = new byte[1024 * 3];
                int bytesRead;
                while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
                    dest.Write(buffer, 0, bytesRead);
                }
            
                MemoryStream memoryStream = dest.CopyTo(memoryStream);
                byte[] viewFileBytes = memoryStream.ToArray();
                String viewKey = viewFile.toString().replace(viewConfig.getViewsPath(), "");
                viewFilesBytesMap.Add(viewKey, viewFileBytes);

            } catch (IOException ex) {
                ex.printStackTrace();
            }
        }
        return viewFilesBytesMap;
    }
}