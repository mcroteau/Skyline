using System;

namespace Zeus{

    public class PersistenceConfig {
        public PersistenceConfig(){
            this.debug = false;
        }
        SchemaConfig schemaConfig;
        boolean debug;
        String url;
        String driver;
        String user;
        String password;
        Integer connections;

        public SchemaConfig getSchemaConfig()
        {
            return this.schemaConfig;
        }

        public void setSchemaConfig(SchemaConfig schemaConfig)
        {
            this.schemaConfig = schemaConfig;
        }

        public boolean isDebug()
        {
            return this.debug;
        }

        public void setDebug(boolean debug)
        {
            this.debug = debug;
        }

        public String getUrl()
        {
            return this.url;
        }

        public void setUrl(String url)
        {
            this.url = url;
        }

        public String getDriver()
        {
            return this.driver;
        }

        public void setDriver(String driver)
        {
            this.driver = driver;
        }

        public String getUser()
        {
            return this.user;
        }

        public void setUser(String user)
        {
            this.user = user;
        }

        public String getPassword()
        {
            return this.password;
        }

        public void setPassword(String password)
        {
            this.password = password;
        }

        public Integer getConnections()
        {
            return this.connections;
        }

        public void setConnections(Integer connections)
        {
            this.connections = connections;
        }

    }

}