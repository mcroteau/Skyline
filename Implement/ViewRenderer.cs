using System;

using AeonFlux;
using AeonFlux.Model;
using AeonFlux.Security;

namespace AeonFlux.Implement {
    public interface ViewRenderer {

        String getKey();

        Boolean isEval();

        Boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

        String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

    }
}