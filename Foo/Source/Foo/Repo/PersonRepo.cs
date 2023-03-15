
using System;
using System.Collections.Generic;

using LiteDB;
using Skyline.Model;
using Skyline.Annotation;

namespace Foo.Repo{

    [Repository]
    public class PersonRepo{
        LiteDatabase db;

        public PersonRepo(){}

        public PersonRepo(ApplicationAttributes applicationAttributes){
            String connectionDatabase = (String)applicationAttributes.getAttributes().GetValueOrDefault("db.url", "");
            this.db = new LiteDatabase("~/FooDb");
        }
    }

}