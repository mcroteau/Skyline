using System;
using AeonFlux;
using AeonFlux.Model;
using AeonFlux.Security;
using AeonFlux.Implement;

namespace AeonFlux{
    public class UsernameRenderer : ViewRenderer {
        public String getKey(){
            return "zeus:user";
        }

        public Boolean isEval(){
            return false;
        }

        public Boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes){
            return false;
        }

        public String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes){
            return "mike@ae0n.net";
        }

    }
}