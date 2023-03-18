using System;

namespace Skyline {
    public class MimeTypeResolver {

        String routeEndpointPath;
        Dictionary<String, String> MIME_MAP;

        public MimeTypeResolver(){
            this.MIME_MAP = new Dictionary<String, String>();
            MIME_MAP.Add("appcache", "text/cache-manifest");
            MIME_MAP.Add("css", "text/css");
            MIME_MAP.Add("gif", "image/gif");
            MIME_MAP.Add("html", "text/html");
            MIME_MAP.Add("js", "application/javascript");
            MIME_MAP.Add("json", "application/json");
            MIME_MAP.Add("jpg", "image/jpeg");
            MIME_MAP.Add("jpeg", "image/jpeg");
            MIME_MAP.Add("mp4", "video/mp4");
            MIME_MAP.Add("mp3", "audio/mp3");
            MIME_MAP.Add("pdf", "application/pdf");
            MIME_MAP.Add("png", "image/png");
            MIME_MAP.Add("svg", "image/svg+xml");
            MIME_MAP.Add("xlsm", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            MIME_MAP.Add("xml", "application/xml");
            MIME_MAP.Add("zip", "application/zip");
            MIME_MAP.Add("md", "text/plain");
            MIME_MAP.Add("txt", "text/plain");
            MIME_MAP.Add("php", "text/plain");
        }


        public String getRouteEndpointPath() {
            return this.routeEndpointPath;
        }

        public void setRouteEndpointPath(String routeEndpointPath) {
            this.routeEndpointPath = routeEndpointPath;
        }

        public Boolean within(String key){
            return MIME_MAP.ContainsKey(key);
        }

        public String resolve() {
            String key = getExt(routeEndpointPath).ToString().ToLower();
            if(MIME_MAP.ContainsKey(key)){
                return MIME_MAP[key];
            }
            return "text/plain";
        }

        public String getExt(String path) {
            int slashIndex = path.LastIndexOf('/');
            int slashIndexWith = slashIndex + 1;
            String basename = (slashIndex < 0) ? path : path.Substring(0, slashIndexWith);

            int dotIdx = basename.LastIndexOf('.');
            if (dotIdx >= 0) {
                int dotIdxWith = dotIdx + 1;
                return basename.Substring(0, dotIdxWith);
            } 
            return "";
        }
    }

}