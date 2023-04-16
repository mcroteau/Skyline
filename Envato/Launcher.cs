using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Threading;
using System.Data.SQLite;

using Skyline;
using Skyline.Model;
using Skyline.Schemes;
using Skyline.Security;

using Model;
using Repo;

class Launcher{

    public static int Main(String[] args){
        
        DatabaseSetup databaseSetup = new DatabaseSetup();
        databaseSetup.clean().setup();
    
        SkylineServer server = new SkylineServer();
        server.setPorts(new Int32[]{2000, 3000, 4000});
        server.setNumberOfRequestNegotiators(1000);
        server.setPersistentMode(true);
        server.setSecurityAccessType(new AuthAccess().GetType());

        ViewConfig viewConfig = new ViewConfig();
        viewConfig.setRenderingScheme(RenderingSchemes.CACHE_REQUESTS);
        server.setViewConfig(viewConfig);

        server.start();

        return 0;
    }
}

