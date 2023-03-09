using System;

using Tiger;
using Tiger.Model;
using Tiger.Security;

namespace Tiger.Implement {
    public interface ViewRenderer {

        String getKey();

        Boolean isEval();

        Boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

        String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

    }
}