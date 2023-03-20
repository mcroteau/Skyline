using System;

namespace Skyline.Model{

    public class RedirectInfo {
        String methodName;
        String klassName;

        public String getMethodName() {
            return this.methodName;
        }

        public void setMethodName(String methodName) {
            this.methodName = methodName;
        }

        public String getKlassName() {
            return this.klassName;
        }

        public void setKlassName(String klassName) {
            this.klassName = klassName;
        }

    }
}