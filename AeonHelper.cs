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

        // Stream dest = new Stream();
        // Stream source = File.OpenRead(path)) {
        //     byte[] buffer = new byte[1024 * 3];
        //     int bytesRead;
        //     while((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0) {
        //         dest.Write(buffer, 0, bytesRead);
        //     }
        // }

    }

    public ConcurrentMap<String, byte[]> getViewBytesMap(ViewConfig viewConfig) {
        ConcurrentMap<String, byte[]> viewFilesBytesMap = new ConcurrentHashMap<>();

        Path viewsPath = Paths.get("src", "main", viewConfig.getViewsPath());
        File viewsDirectory = new File(viewsPath.toString());

        if(viewsDirectory.isDirectory()) {
            File[] viewFiles = viewsDirectory.listFiles();
            getFileBytesMap(viewFiles, viewConfig, viewFilesBytesMap);
        }

        Path resourcesPath = Paths.get("src", "main", viewConfig.getViewsPath(), viewConfig.getResourcesPath());
        File resourcesDirectory = new File(resourcesPath.toString());

        if(resourcesDirectory.isDirectory()) {
            File[] resourceFiles = resourcesDirectory.listFiles();
            getFileBytesMap(resourceFiles, viewConfig, viewFilesBytesMap);
        }

        return viewFilesBytesMap;
    }

    Dictionary<String, byte[]> getFileBytesMap(File[] viewFiles, ViewConfig viewConfig, Dictionary<String, byte[]> viewFilesBytesMap){
        foreach(File viewFile in viewFiles) {

            if(viewFile.isDirectory()){
                File[] directoryFiles = viewFile.listFiles();
                getFileBytesMap(directoryFiles, viewConfig, viewFilesBytesMap);
                continue;
            }

            InputStream fileInputStream = new FileInputStream(viewFile);
            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            byte[] bytes = new byte[1024];
            int bytesRead;
            try {
                while ((bytesRead = fileInputStream.read(bytes, 0, bytes.length)) != -1) {
                    outputStream.write(bytes, 0, bytesRead);
                }

                fileInputStream.close();
                outputStream.flush();
                outputStream.close();

                byte[] viewFileBytes = outputStream.toByteArray();
                String viewKey = viewFile.toString().replace("src" + File.separator + "main" + File.separator + viewConfig.getViewsPath(), "");
                viewFilesBytesMap.put(viewKey, viewFileBytes);

            } catch (IOException ex) {
                ex.printStackTrace();
            }
        }
        return viewFilesBytesMap;
    }
}