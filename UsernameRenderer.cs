using System;
using Skyline;
using Skyline.Model;
using Skyline.Security;
using Skyline.Implement;

namespace Skyline{
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