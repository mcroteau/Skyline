using System;

namespace Zeus{
    public interface ViewRenderer {

        public String getKey();

        /**
        * @return true if conditional snipit, false if content is rendered.
        */
        public Boolean isEval();

        public boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

        public String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

    }
}