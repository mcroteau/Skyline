using System;

using Skyline.Model;

namespace Skyline{
    public class NetworkRequestHeaderResolver{
        String BREAK = "\r\n";
        String networkRequestHeaderElement;
        NetworkRequest networkRequest;

        public void resolve(){
            String[] networkRequestHeaderElements = networkRequestHeaderElement.Split(BREAK);
            foreach(String headerLineElement in networkRequestHeaderElements){
                String[] headerLineComponents = headerLineElement.Split(":", 2);
                Console.WriteLine("req=>" + networkRequest.getRequestPath() + "     /===> " + headerLineElement);
                if(headerLineComponents.Length == 2) {
                    String fieldKey = headerLineComponents[0].Trim();
                    String content = headerLineComponents[1].Trim();
                    networkRequest.getHeaders().Add(fieldKey.ToLower(), content);
                }
            }
        }

        public String getNetworkRequestHeaderElement() {
            return this.networkRequestHeaderElement;
        }

        public void setNetworkRequestHeaderElement(String networkRequestHeaderElement) {
            this.networkRequestHeaderElement = networkRequestHeaderElement;
        }

        public NetworkRequest getNetworkRequest() {
            return this.networkRequest;
        }

        public void setNetworkRequest(NetworkRequest networkRequest) {
            this.networkRequest = networkRequest;
        }
    }
}