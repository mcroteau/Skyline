using System;
using System.Text;
using System.Web;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;

using Skyline.Model;

namespace Skyline{
    
    public class RequestComponentResolver {

        Encoding encoding;
        String requestPayload;
        String queryString;
        NetworkRequest networkRequest;

        public void resolve(){

            try {
                var utf8 = new UTF8Encoding();

                Dictionary<String, String> headers = networkRequest.getHeaders();

                if(headers.ContainsKey("content-type")){
                    
                    String contentType = headers["content-type"];
                    String[] boundaryParts = contentType != null ? contentType.Split("boundary=") : new String[]{};

                    if (boundaryParts.Length > 1) {
                        String formDelimiter = boundaryParts[1].Replace(";", "");
                        ArrayList requestComponents = getRequestComponents(formDelimiter);
                        
                        foreach(RequestComponent requestComponent in requestComponents){
                            String requestComponentKey = requestComponent.getName();
                            networkRequest.setRequestComponent(requestComponentKey, requestComponent);
                        }
                    }else{

                        String requestQueryFinal = HttpUtility.UrlDecode(requestPayload);

                        if(!requestQueryFinal.Equals("")){
                            String[] requestQueryParts = requestQueryFinal.Split("&");
                            foreach(String entry in requestQueryParts) {
                                RequestComponent requestComponent = new RequestComponent();
                                String[] keyValue = entry.Split("=", 2);
                                String key = keyValue[0].Trim();
                                String keyNoClue = new String(key.Where(c => !char.IsControl(c)).ToArray());
                                if (keyValue.Length > 1) {
                                    String value = keyValue[1].Trim();
                                    String valueNoIdea = new String(value.Where(c => !char.IsControl(c)).ToArray());
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

        ArrayList getRequestComponents(String delimeter){
            String elementRegex = "(Content-Disposition: form-data; name=\"[a-zA-Z\\-\\._\\d]+\"$)|(Content-Disposition: form-data; name=\"[a-zA-Z\\-\\.\\d]+\"; filename=\"[a-zA-Z\\.\\-_\\s\\d\\']+\")";

            Dictionary<String, Boolean> matches = new Dictionary<String, Boolean>();
            ArrayList components = new ArrayList();

            int lastIndex = 3;

            foreach(Match match in Regex.Matches(requestPayload, elementRegex,
                                               RegexOptions.None,
                                               TimeSpan.FromSeconds(1))){
                              
                String fileGroup = match.Value;

                int beginIndex = requestPayload.IndexOf(fileGroup, lastIndex);
                int beginIndexWith = beginIndex + fileGroup.Length;
                int delimiterIndex = requestPayload.IndexOf(delimeter, lastIndex);
                int delimiterIndexWith = delimiterIndex + delimeter.Length;
                int componentLength = delimiterIndexWith - beginIndex;

                String componentContent = requestPayload.Substring(beginIndex, componentLength);

                Component component = new Component(componentContent);
                component.setActiveBeginIndex(beginIndex);
                component.setActiveCloseIndex(delimiterIndex);
                components.Add(component);
                lastIndex = delimiterIndexWith;

            }

            String NAME = "name=\"";
            String FILE = "filename=\"";
            String NEWLINE = "\r\n";
            Dictionary<String, RequestComponent> requestComponentMap = new Dictionary<String, RequestComponent>();
            foreach(Component component in components){
                String componentContent = component.getComponent();
                int beginNameIdx = componentContent.IndexOf(NAME);
                int beginNameIdxWith = beginNameIdx + NAME.Length;
                int endNameIdx = componentContent.IndexOf("\"", beginNameIdxWith);
                int nameDiff = endNameIdx - beginNameIdx;
                String nameElement = componentContent.Substring(beginNameIdxWith, nameDiff);

                int beginFilenameIdx = componentContent.IndexOf(FILE);
                if(beginFilenameIdx == -1){
                    int beginValueIdx = componentContent.IndexOf("\r\n\r\n", endNameIdx);
                    int beginValueIdxWith = beginValueIdx + "\r\n\r\n".Length;

                    String endDelimeter = delimeter + "--";

                    int endValueIdx = componentContent.IndexOf(endDelimeter, beginValueIdxWith);
                    int endValueIdxWith = endValueIdx + endDelimeter.Length;
                    int valueDiff = endValueIdxWith - beginValueIdxWith;
                    String valueDirty = componentContent.Substring(beginValueIdxWith, valueDiff);
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
                        FileComponent fileComponent = getFileComponent(delimeter, component, componentContent);
                        if(fileComponent != null) {
                            requestComponent.getFileComponents().Add(fileComponent);
                            requestComponentMap[nameElement] = requestComponent;
                        }
                    }else{
                        RequestComponent requestComponent = new RequestComponent();
                        requestComponent.setName(nameElement);
                        FileComponent fileComponent = getFileComponent(delimeter, component, componentContent);
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

        FileComponent getFileComponent(String delimeter, Component component, String componentContent) {
            FileComponent fileComponent = new FileComponent();

            int fileIdx = componentContent.IndexOf("filename=");
            int fileIdxWith = fileIdx + 1;
            
            int startFile = componentContent.IndexOf("\"", fileIdxWith);
            int startFileWith = startFile + 1;

            int endFile = componentContent.IndexOf("\"", startFileWith);
            int fileDiff = endFile - startFileWith;

            String fileName = componentContent.Substring(startFileWith, fileDiff);
            fileComponent.setFileName(fileName);

            int endFileWith = endFile + 1;

            int startContent = componentContent.IndexOf("Content-Type", endFileWith);
            int startContentWith = startContent + 1;

            int startType = componentContent.IndexOf(":", startContentWith);
            int startTypeWith = startType + 1;

            int endType = componentContent.IndexOf("\r\n", startTypeWith);
            int endTypeWith = endType + "\r\n".Length;
            int typeDiff = endType - startTypeWith;

            String contentType = componentContent.Substring(startTypeWith, typeDiff).Trim();
            fileComponent.setContentType(contentType);

            int activeCloseIndex = componentContent.IndexOf(delimeter);
            int fileValueDiff = activeCloseIndex - endTypeWith + "\r\n".Length;

            String fileValue = componentContent.Substring(endTypeWith, fileValueDiff);
            // String fileValueClean = new String(fileValue.Where(c => !char.IsControl(c)).ToArray());
            int dashIndex = fileValue.LastIndexOf("----");  
            String peace  = fileValue;
            if(dashIndex != -1){          
                peace = fileValue.Remove(dashIndex, "----".Length);
            }
            int newlineIndex = peace.LastIndexOf("\r\n");  
            String love  = peace;
            if(newlineIndex != -1){          
                love = peace.Remove(newlineIndex, "\r\n".Length);
            }
            int firstNewlineIndex = love.IndexOf("\r\n");  
            String harmony  = love;
            if(firstNewlineIndex != -1){          
                harmony = love.Remove(firstNewlineIndex, "\r\n".Length);
            }

            ArrayList fileBytesArray = new ArrayList();
            char[] fileChars = harmony.ToCharArray();
            for(int xyz = 0; xyz < fileChars.Length; xyz++){
                fileBytesArray.Add((byte)fileChars[xyz]);
            }

            byte[] fileBytes = new byte[fileBytesArray.Count];
            for(int xyz = 0; xyz < fileBytesArray.Count; xyz++){
                fileBytes[xyz] = (byte)fileBytesArray[xyz];
            }

            using var writer = new BinaryWriter(File.OpenWrite("copy-" + fileName));
            writer.Write(fileBytes);

            fileComponent.setFileBytes(fileBytes);
            fileComponent.setActiveIndex(activeCloseIndex);

            return fileComponent;
        
        }
        
        public void setEncoding(Encoding encoding){
            this.encoding = encoding;
        }

        public void setQueryString(String queryString){
            this.queryString = queryString;
        }

        public void setRequestPayload(String requestPayload){
            this.requestPayload = requestPayload;
        }

        public NetworkRequest getNetworkRequest() {
            return networkRequest;
        }

        public void setNetworkRequest(NetworkRequest networkRequest) {
            this.networkRequest = networkRequest;
        }
    }
}