using System;
using System.Text;
using Skyline.Model;

namespace Skyline{
    public class RequestHeaderResolver{
        String BREAK = "\r\n";
        NetworkRequest networkRequest;

        public void resolve(){
            foreach (String key in networkRequest.getContext().Request.Headers.AllKeys){
                String[] values = networkRequest.getContext().Request.Headers.GetValues(key);
                if(values.Length > 0){
                    StringBuilder Sb = new StringBuilder();
                    foreach (String value in values){
                        Sb.Append(value + ";");
                    }
                    networkRequest.getHeaders()[key.ToLower()] = Sb.ToString();
                }
            }
        }

        public void setNetworkRequest(NetworkRequest networkRequest) {
            this.networkRequest = networkRequest;
        }
    }
}