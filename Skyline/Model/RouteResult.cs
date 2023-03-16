using System;
using System.Collections.Generic;
namespace Skyline.Model{
    public class RouteResult {
        public RouteResult(Dictionary<String, Object> redirectAttributes) {
            this.redirectAttributes = redirectAttributes;
        }

        public RouteResult(byte[] responseBytes, String responseCode, String contentType) {
            this.responseBytes = responseBytes;
            this.responseCode = responseCode;
            this.contentType = contentType;
        }

        public RouteResult() { }

        Boolean completeRequest;
        byte[] responseBytes;
        String contentType;
        String responseCode;

        Dictionary<String, Object> redirectAttributes;

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

        public Dictionary<String, Object> getRedirectAttributes() {
            return this.redirectAttributes;
        }

        public void setRedirectAttributes(Dictionary<String, Object> redirectAttributes) {
            this.redirectAttributes = redirectAttributes;
        }
    }
}