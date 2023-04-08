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

using Persistence.Model;
using Persistence.Repo;

namespace Persistence{

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
            server.setPersistentMode(true);
            server.setSecurityAccessType(new AuthAccess().GetType());
            server.start();

            return 0;
        }
    }
}

