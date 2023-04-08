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

using Driven.Model;
using Driven.Repo;

namespace Driven {

    class Launcher{

        public static int Main(String[] args){
            
            DatabaseSetup databaseSetup = new DatabaseSetup();
            SQLiteConnection connection = databaseSetup.getConnection();
            databaseSetup.clean();
            databaseSetup.setup();

            ApplicationAttributes applicationAttributes = new ApplicationAttributes();
            DataTransferObject dto = new DataTransferObject(new PersistenceConfig());
            dto.setApplicationAttributes(applicationAttributes);
            
            UserRepo userRepo = new UserRepo(dto);
            PersistenceConfig persistenceConfig = new PersistenceConfig();
            SecurityManager manager = new SecurityManager(new AuthAccess(new DataTransferObject(persistenceConfig)));

            User user = new User();
            user.setEmail("abc@plsar.net");
            user.setPassword("effort.");
            long id = userRepo.save(user);

            SkylineServer server = new SkylineServer(4000, 70);

            Type authAccessKlassType = new AuthAccess().GetType();
            server.setSecurityAccessType(authAccessKlassType);

            ViewConfig viewConfig = new ViewConfig();
            viewConfig.setResourcesPath("assets");
            viewConfig.setViewsPath("pages");
            server.setViewConfig(viewConfig);

            server.start();

            return 0;
        }
    }
}

