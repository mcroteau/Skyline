using System;

namespace Zeus{
    public interface ViewRenderer {

        String getKey();

        Boolean isEval();

        Boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

        String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

    }
}