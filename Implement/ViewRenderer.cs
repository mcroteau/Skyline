using System;

using Skyline;
using Skyline.Model;
using Skyline.Security;

namespace Skyline.Implement {
    public interface ViewRenderer {

        String getKey();

        Boolean isEval();

        Boolean truthy(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

        String render(NetworkRequest networkRequest, SecurityAttributes securityAttributes);

    }
}