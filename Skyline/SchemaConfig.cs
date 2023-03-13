using System;

namespace Skyline{
    public class SchemaConfig {
        String schema;
        String environment;

        public String getSchema()
        {
            return this.schema;
        }

        public void setSchema(String schema)
        {
            this.schema = schema;
        }

        public String getEnvironment()
        {
            return this.environment;
        }

        public void setEnvironment(String environment)
        {
            this.environment = environment;
        }

    }
}