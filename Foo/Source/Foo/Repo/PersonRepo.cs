
using System;


namespace Foo{

    [Repository]
    public class PersonRepo{
        LiteDB db;

        public PersonRepo(ApplicationAttributes applicationAttributes){
            String connectionDatabase = applicationAttributes.getAttributes().GetValue("db.url");
            db = new LiteDatabase("~/FooDb");
        }

        public void save(Person person){
            db
        }

    }

}