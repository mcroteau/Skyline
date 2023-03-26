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

using Foo.Model;
using Foo.Repo;

namespace Foo{
    class Launcher{

        public static int Main(String[] args){
            
            DatabaseSetup databaseSetup = new DatabaseSetup();
            SQLiteConnection connection = databaseSetup.getConnection();
            databaseSetup.clean();
            databaseSetup.setup();

            ApplicationAttributes applicationAttributes = new ApplicationAttributes();
            applicationAttributes.getAttributes().Add("connection", connection);

            DataTransferObject dto = new DataTransferObject(new PersistenceConfig());
            dto.setApplicationAttributes(applicationAttributes);
            
            PersonRepo personRepo = new PersonRepo(dto);
            PersistenceConfig persistenceConfig = new PersistenceConfig();
            SecurityManager manager = new SecurityManager(new AuthAccess(new DataTransferObject(persistenceConfig)));

            User user = new User();
            user.setEmail("abc@plsar.net");
            user.setPassword("effort.");
            long id = personRepo.save(user);

            SkylineRunnable skyline = new SkylineRunnable(4000);
            skyline.setNumberOfPartitions(30);
            skyline.setNumberOfRequestExecutors(70);


            ViewConfig viewConfig = new ViewConfig();
            viewConfig.setResourcesPath("Assets");
            viewConfig.setViewsPath("Views");
            viewConfig.setRenderingScheme(RenderingScheme.RELOAD_EACH_REQUEST);

            skyline.setSecurityAccessType(new AuthAccess().GetType());

            skyline.setApplicationAttributes(applicationAttributes);
            skyline.setPersistenceConfig(persistenceConfig);
            skyline.setViewConfig(viewConfig);

            skyline.start();
            return 0;
        }
    }
}

