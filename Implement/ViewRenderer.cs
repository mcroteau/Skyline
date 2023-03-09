using System;

using Zeus;
using Zeus.Model;
using Zeus.Security;

namespace Zeus.Implement {
    public interface ViewRenderer {

        String getKey();

        Boolean isEval();

        Boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

        String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

    }
}