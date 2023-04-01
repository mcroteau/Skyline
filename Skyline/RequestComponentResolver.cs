using System;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections;

using Skyline.Model;

namespace Skyline{
    
    public class RequestComponentResolver {

        byte[] requestBytes;
        NetworkRequest networkRequest;

        public void resolve(){

            try {
                var utf8 = new UTF8Encoding();

                Dictionary<String, String> headers = networkRequest.getHeaders();

                if(headers.ContainsKey("content-type")){
                    
                    String contentType = headers["content-type"];
                    String[] boundaryParts = contentType != null ? contentType.Split("boundary=") : new String[]{};

                    Console.WriteLine("boundaryParts:" + boundaryParts.Length);
                    if (boundaryParts.Length > 1) {
                        String formDelimiter = boundaryParts[1];
                        String requestPayload = getRequestContent(requestBytes);
                        ArrayList requestComponents = getRequestComponents(formDelimiter, requestPayload);
                        
                        foreach(RequestComponent requestComponent in requestComponents){
                            String requestComponentKey = requestComponent.getName();
                            networkRequest.setRequestComponent(requestComponentKey, requestComponent);
                        }
                    }else if(requestBytes.Length > 0){

                        String requestQueryComplete = utf8.GetString(requestBytes);
                        String requestQueryFinal = HttpUtility.UrlDecode(requestQueryComplete);
                        String[] requestBodyQuery = requestQueryFinal.Split("\r\n\r\n", 2);

                        if(requestBodyQuery.Length == 2 &&
                                !requestBodyQuery[1].Equals("")) {
                            String requestQuery =  requestBodyQuery[1];
                            String[] requestQueryParts = requestQuery.Split("&");
                            foreach(String entry in requestQueryParts) {
                                RequestComponent requestComponent = new RequestComponent();
                                String[] keyValue = entry.Split("=", 2);
                                String key = keyValue[0].Trim();
                                string keyNoClue = new string(key.Where(c => !char.IsControl(c)).ToArray());
                                if (keyValue.Length > 1) {
                                    String value = keyValue[1].Trim();
                                    string valueNoIdea = new string(value.Where(c => !char.IsControl(c)).ToArray());
                                    requestComponent.setName(keyNoClue);
                                    requestComponent.setValue(valueNoIdea);
                                } else {
                                    requestComponent.setName(keyNoClue);
                                    requestComponent.setValue("");
                                }
                                networkRequest.getRequestComponents()[keyNoClue] = requestComponent;
                            }
                        }
                    }

                }

            } catch (Exception ex){
                Console.WriteLine(ex.ToString());
            }
        }

        ArrayList getRequestComponents(String delimeter, String requestPayload){
            String elementRegex = "(Content-Disposition: form-data; name=\"[a-zA-Z\\-\\._\\d]+\"\\s)|(Content-Disposition: form-data; name=\"[a-zA-Z\\-\\.\\d]+\"; filename=\"[a-zA-Z\\.\\-_\\s\\d\\']+\")";

            Dictionary<String, Boolean> matches = new Dictionary<String, Boolean>();
            ArrayList components = new ArrayList();

            int lastIndex = 0;
            Regex regexLocator = new Regex(elementRegex, RegexOptions.IgnoreCase);
            Match matcher = regexLocator.Match(requestPayload);
            
            while (matcher.Success){

                String fileGroup = matcher.Value;
                if(!matches.ContainsKey(fileGroup)){

                    Console.WriteLine("zz:" + fileGroup);

                    int beginIndex = requestPayload.IndexOf(fileGroup, lastIndex);
                    int delimeterIndexLength = beginIndex + delimeter.Length;
                    int delimiterIndex = requestPayload.IndexOf(delimeter, beginIndex);
                    int delimeterLength = delimeter.Length;
                    Console.WriteLine("za:" + delimiterIndex);
                    String componentContent = requestPayload.Substring(delimiterIndex, delimiterIndex);
                    
                    Console.WriteLine("component content " + componentContent);

                    Component component = new Component(componentContent);
                    component.setActiveBeginIndex(beginIndex);
                    component.setActiveCloseIndex(delimiterIndex);
                    components.Add(component);
                    lastIndex = delimiterIndex;

                    matches[fileGroup] = true;
                }

            }

            String NAME = "name=\"";
            String FILE = "filename=\"";
            String NEWLINE = "\r\n";
            Dictionary<String, RequestComponent> requestComponentMap = new Dictionary<String, RequestComponent>();
            foreach(Component component in components){
                String componentContent = component.getComponent();
                Console.WriteLine("component:" + component);
                int beginNameIdx = componentContent.IndexOf(NAME);
                int endNameIdx = componentContent.IndexOf("\"", beginNameIdx + NAME.Length);
                String nameElement = componentContent.Substring(beginNameIdx + NAME.Length, endNameIdx);

                int beginFilenameIdx = componentContent.IndexOf(FILE);
                if(beginFilenameIdx == -1){
                    int beginValueIdx = componentContent.IndexOf("\r\n\r\n", endNameIdx);
                    int endValueIdx = componentContent.IndexOf("--", beginValueIdx + NEWLINE.Length);
                    String valueDirty = componentContent.Substring(beginValueIdx + NEWLINE.Length, endValueIdx);
                    String value = valueDirty.Replace("\r\n", "");

                    if(requestComponentMap.ContainsKey(nameElement)){
                        RequestComponent requestComponent = requestComponentMap[nameElement];
                        requestComponent.setName(nameElement);
                        requestComponent.setValue(value);
                        requestComponent.getValues().Add(value);
                        requestComponentMap[nameElement] = requestComponent;
                    }else{
                        RequestComponent requestComponent = new RequestComponent();
                        requestComponent.setName(nameElement);
                        requestComponent.setValue(value);
                        requestComponent.getValues().Add(value);
                        requestComponentMap[nameElement] = requestComponent;
                    }

                }else{
                    if(requestComponentMap.ContainsKey(nameElement)){
                        RequestComponent requestComponent = requestComponentMap[nameElement];
                        requestComponent.setName(nameElement);
                        FileComponent fileComponent = getFileComponent(component, componentContent);
                        if(fileComponent != null) {
                            requestComponent.getFileComponents().Add(fileComponent);
                            requestComponentMap[nameElement] = requestComponent;
                        }
                    }else{
                        RequestComponent requestComponent = new RequestComponent();
                        requestComponent.setName(nameElement);
                        FileComponent fileComponent = getFileComponent(component, componentContent);
                        if(fileComponent != null) {
                            requestComponent.getFileComponents().Add(fileComponent);
                            requestComponentMap[nameElement] = requestComponent;
                        }
                    }
                }
            }

            ArrayList requestComponents = new ArrayList();
            foreach(var requestComponentEntry in requestComponentMap){
                requestComponents.Add(requestComponentEntry.Value);
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
            String type = componentContent.Substring(startType + 1, endType).Trim();
            fileComponent.setContentType(type);

            int activeBeginIndex = component.getActiveBeginIndex() + componentContent.IndexOf("\r\n", endType) + "\r\n\r\n".Length;
            int activeCloseIndex = component.getActiveCloseIndex() + componentContent.IndexOf("--", activeBeginIndex);

            if(activeCloseIndex >= requestBytes.Length)activeCloseIndex = requestBytes.Length;

            if (activeCloseIndex - activeBeginIndex > "\r\n\r\n".Length) {

                ArrayList byteArrayOutputStream = new ArrayList();
                for (int activeIndex = activeBeginIndex; activeIndex < activeCloseIndex; activeIndex++) {
                    byte activeByte = requestBytes[activeIndex];
                    byteArrayOutputStream.Add(activeByte);
                }

                int index = 0;
                byte[] bytes = new byte[byteArrayOutputStream.Count];
                foreach(object objectInstance in byteArrayOutputStream){
                    byte b = (byte)objectInstance;
                    bytes[index] = b;
                }

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
            return  sb.ToString();
        }

    }
}