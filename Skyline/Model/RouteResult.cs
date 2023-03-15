using System;
namespace Skyline.Model{
    public class RouteResult {

        Boolean completeRequest;
        byte[] responseBytes;
        String contentType;
        String responseCode;

        public Boolean getCompleteRequest() {
            return this.completeRequest;
        }

        public void setCompleteRequest(Boolean completeRequest) {
            this.completeRequest = completeRequest;
        }

        public byte[] getResponseBytes() {
            return this.responseBytes;
        }

        public void setResponseBytes(byte[] responseBytes) {
            this.responseBytes = responseBytes;
        }

        public String getContentType() {
            return this.contentType;
        }

        public void setContentType(String contentType) {
            this.contentType = contentType;
        }

        public String getResponseCode() {
            return this.responseCode;
        }

        public void setResponseCode(String responseCode) {
            this.responseCode = responseCode;
        }
    }
}