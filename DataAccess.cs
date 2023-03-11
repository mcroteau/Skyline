using System;
using LiteDB;

namespace AeonFlux{
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