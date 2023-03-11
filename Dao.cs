using System;
using LiteDB;

namespace Tiger{
    public class Dao{
        
        LiteDatabase db;

        public Dao(string databaseUri){
            this.db = new LiteDatabase(databaseUri);
        }

        public LiteDatabase getDb(){
            return this.db;
        }
    }
}