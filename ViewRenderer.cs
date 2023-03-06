using System;

namespace Zeus{
    public interface ViewRenderer {

        String getKey();

        /**
        * @return true if conditional snipit, false if content is rendered.
        */
        Boolean isEval();

        Boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

        String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

    }
}