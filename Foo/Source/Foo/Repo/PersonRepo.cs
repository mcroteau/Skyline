
using System;
using System.Collections.Generic;

using Skyline;
using Skyline.Model;
using Skyline.Annotation;

namespace Foo.Repo{

    [Repository]
    public class PersonRepo{

        public PersonRepo(){}

        public PersonRepo(DataTransferObject dto, ApplicationAttributes applicationAttributes){}
        
        public PersonRepo(ApplicationAttributes applicationAttributes){}

        public void print(){
            Console.WriteLine("dto in da house!");
        }
    }

}