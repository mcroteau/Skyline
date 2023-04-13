using System;

namespace Skyline {
    public class MimeResolver {

        String routeEndpointPath;
        Dictionary<String, String> MIME_MAP;

        public MimeResolver(){
            this.MIME_MAP = new Dictionary<String, String>();
            this.MIME_MAP["appcache"] = "text/cache-manifest";
            this.MIME_MAP["css"] = "text/css";
            this.MIME_MAP["gif"] = "image/gif";
            this.MIME_MAP["html"] = "text/html";
            this.MIME_MAP["js"] = "application/javascript";
            this.MIME_MAP["json"] = "application/json";
            this.MIME_MAP["jpg"] ="image/jpeg";
            this.MIME_MAP["jpeg"] ="image/jpeg";
            this.MIME_MAP["mp4"] ="video/mp4";
            this.MIME_MAP["mp3"] ="audio/mp3";
            this.MIME_MAP["pdf"] ="application/pdf";
            this.MIME_MAP["png"] ="image/png";
            this.MIME_MAP["svg"] ="image/svg+xml";
            this.MIME_MAP["xlsm"] ="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            this.MIME_MAP["xml"] ="application/xml";
            this.MIME_MAP["zip"] ="application/zip";
            this.MIME_MAP["md"] ="text/plain";
            this.MIME_MAP["txt"] ="text/plain";
            this.MIME_MAP["php"] ="text/plain";
        }


        public String getRouteEndpointPath() {
            return this.routeEndpointPath;
        }

        public void setRouteEndpointPath(String routeEndpointPath) {
            this.routeEndpointPath = routeEndpointPath;
        }

        public String resolve() {
            String key = getExt(routeEndpointPath).ToString().ToLower();
            if(MIME_MAP.ContainsKey(key)){
                return MIME_MAP[key];
            }
            return "text/plain";
        }

        public String getExt(String path) {
            int slashIndex = path.LastIndexOf('.');
            int slashIndexWith = slashIndex + 1;
            int extDiff = path.Length - slashIndexWith;
            String basename = path.Substring(slashIndexWith, extDiff);
            return basename;
        }
    }

}