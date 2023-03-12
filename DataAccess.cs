using System;
using LiteDB;

namespace Skyline{
    public class DataAccess{
        
        LiteDatabase db;

        public DataAccess(string databaseUri){
            this.db = new LiteDatabase(databaseUri);
        }

        public LiteDatabase getDb(){
            return this.db;
        }
    }
}