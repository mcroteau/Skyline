using System;
using System.Collections;

using Skyline.Model;

namespace Skyline{
    
    public class RequestComponentResolver {

        byte[] requestBytes;
        NetworkRequest networkRequest;

        public void resolve(){
            Dictionary<String, String> headers = networkRequest.getHeaders();
            String contentType = headers.get("content-type");
            String[] boundaryParts = contentType != null ? contentType.Split("boundary=") : new String[]{};

            if (boundaryParts.Length > 1) {
                String formDelimiter = boundaryParts[1];
                String requestPayload = getRequestContent(requestBytes);
                ArrayList requestComponents = getRequestComponents(formDelimiter, requestPayload);
                
                foreach(var requestComponent in requestComponents){
                    String requestComponentKey = requestComponent.getName();
                    networkRequest.setRequestComponent(requestComponentKey, requestComponent);
                }
            }else if(requestBytes.length > 0){

                try {

                    String queryBytes = new String(requestBytes, "utf-8");
                    String requestQueryComplete = java.net.URLDecoder.decode(queryBytes, StandardCharsets.UTF_8.name());
                    String[] requestQueryParts = requestQueryComplete.Split("\r\n\r\n", 2);
                    if(requestQueryParts.length == 2 &&
                            !requestQueryParts[1].equals("")) {
                        String requestQuery =  requestQueryParts[1];
                        foreach(String entry in requestQuery.Split("&")) {
                            RequestComponent requestComponent = new RequestComponent();
                            String[] keyValue = entry.Split("=", 2);
                            String key = keyValue[0];
                            if (keyValue.length > 1) {
                                String value = keyValue[1];
                                requestComponent.setName(key);
                                requestComponent.setValue(value);
                            } else {
                                requestComponent.setName(key);
                                requestComponent.setValue("");
                            }
                            networkRequest.put(key, requestComponent);
                        }
                    }

                } catch (Exception ex){
                    ex.printStackTrace();
                }
            }
        }

        ArrayList getRequestComponents(String delimeter, String requestPayload){
            String elementRegex = "(Content-Disposition: form-data; name=\"[a-zA-Z\\-\\._\\d]+\"\\s)|(Content-Disposition: form-data; name=\"[a-zA-Z\\-\\.\\d]+\"; filename=\"[a-zA-Z\\.\\-_\\s\\d\\']+\")";

            ArrayList components = new ArrayList();

            Integer lastIndex = 0;
            Regex regexLocator = new Regex(elementRegex, RegexOptions.IgnoreCase);
            Match matcher = regexLocator.Match(requestPayload);
            while (matcher.Success){
                String fileGroup = elementMatcher.group();
                Console.WriteLine("zz:" + fileGroup);
                int beginIndex = requestPayload.IndexOf(fileGroup, lastIndex);
                int beginIdexWith = beginIndex + 1;
                int delimiterIndex = requestPayload.IndexOf(delimeter, beginIndexWith);
                int delimeterTotal = delimiterIndex + delimeter.Length;
                Console.WriteLine("za:" + delimiterIndex);
                String componentContent = requestPayload.substring(beginIndex, delimiterIndex);
                Component component = new Component(componentContent);
                component.setActiveBeginIndex(beginIndex);
                component.setActiveCloseIndex(delimeterTotal);
                components.add(component);
                lastIndex = delimiterIndex;
            }

            String NAME = "name=\"";
            String FILE = "filename=\"";
            String NEWLINE = "\r\n";
            Dictionary<String, RequestComponent> requestComponentMap = new Dictionary<String, RequestComponent>();
            foreach(Component component in components){
                String componentContent = component.getComponent();
                Console.WriteLine("component:" + component);
                int beginNameIdx = componentContent.IndexOf(NAME);
                int endNameIdx = componentContent.IndexOf("\"", beginNameIdx + NAME.length());
                String nameElement = componentContent.substring(beginNameIdx + NAME.length(), endNameIdx);

                int beginFilenameIdx = componentContent.IndexOf(FILE);
                if(beginFilenameIdx.compareTo(-1) == 0){
                    int beginValueIdx = componentContent.IndexOf("\r\n\r\n", endNameIdx);
                    int endValueIdx = componentContent.IndexOf("--", beginValueIdx + NEWLINE.length());
                    String valueDirty = componentContent.substring(beginValueIdx + NEWLINE.length(), endValueIdx);
                    String value = valueDirty.replace("\r\n", "");

                    if(requestComponentMap.containsKey(nameElement)){
                        RequestComponent requestComponent = requestComponentMap[nameElement];
                        requestComponent.setName(nameElement);
                        requestComponent.setValue(value);
                        requestComponent.getValues().add(value);
                        requestComponentMap.replace(nameElement, requestComponent);
                    }else{
                        RequestComponent requestComponent = new RequestComponent();
                        requestComponent.setName(nameElement);
                        requestComponent.setValue(value);
                        requestComponent.getValues().add(value);
                        requestComponentMap.put(nameElement, requestComponent);
                    }

                }else{
                    if(requestComponentMap.ContainsKey(nameElement)){
                        RequestComponent requestComponent = requestComponentMap[nameElement];
                        requestComponent.setName(nameElement);
                        FileComponent fileComponent = getFileComponent(component, componentContent);
                        if(fileComponent != null) {
                            requestComponent.getFileComponents().Add(fileComponent);
                            requestComponentMap.put(nameElement, requestComponent);
                        }
                    }else{
                        RequestComponent requestComponent = new RequestComponent();
                        requestComponent.setName(nameElement);
                        FileComponent fileComponent = getFileComponent(component, componentContent);
                        if(fileComponent != null) {
                            requestComponent.getFileComponents().Add(fileComponent);
                            requestComponentMap.put(nameElement, requestComponent);
                        }
                    }
                }
            }

            ArrayList requestComponents = new ArrayList();
            foreach(var requestComponentEntry in requestComponentMap){
                requestComponents.Add(requestComponentEntry.getValue());
            }

            return requestComponents;
        }

        FileComponent getFileComponent(Component component, String componentContent) {
            FileComponent fileComponent = new FileComponent();

            int fileIdx = componentContent.IndexOf("filename=");
            int startFile = componentContent.IndexOf("\"", fileIdx + 1);
            int endFile = componentContent.IndexOf("\"", startFile + 1);
            String fileName = componentContent.Substring(startFile + 1, endFile);
            fileComponent.setFileName(fileName);

            int startContent = componentContent.IndexOf("Content-Type", endFile + 1);
            int startType = componentContent.IndexOf(":", startContent + 1);
            int endType = componentContent.IndexOf("\r\n", startType + 1);
            String type = componentContent.Substring(startType + 1, endType).trim();
            fileComponent.setContentType(type);

            int activeBeginIndex = component.getActiveBeginIndex() + componentContent.IndexOf("\r\n", endType) + "\r\n\r\n".length();
            int activeCloseIndex = component.getActiveCloseIndex() + componentContent.IndexOf("--", activeBeginIndex);

            if(activeCloseIndex >= requestBytes.length)activeCloseIndex = requestBytes.length;

            if (activeCloseIndex - activeBeginIndex > "\r\n\r\n".length()) {

                ArrayList byteArrayOutputStream = new ArrayList();
                for (int activeIndex = activeBeginIndex; activeIndex < activeCloseIndex; activeIndex++) {
                    byte activeByte = requestBytes[activeIndex];
                    byteArrayOutputStream.Add(activeByte);
                }

                byte[] bytes = byteArrayOutputStream.ToArray();
                fileComponent.setFileBytes(bytes);
                fileComponent.setActiveIndex(activeCloseIndex);

                return fileComponent;
            }

            return null;
        }

        public byte[] getRequestBytes() {
            return requestBytes;
        }

        public void setRequestBytes(byte[] requestBytes) {
            this.requestBytes = requestBytes;
        }

        public NetworkRequest getNetworkRequest() {
            return networkRequest;
        }

        public void setNetworkRequest(NetworkRequest networkRequest) {
            this.networkRequest = networkRequest;
        }

        String getRequestContent(byte[] requestBytes){
            StringBuilder sb = new StringBuilder();
            foreach(byte b in requestBytes) {
                sb.Append((char) b);
            }
            return  sb.toString();
        }

    }
}