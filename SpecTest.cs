using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

using Zeus;
using Zeus.Model;
using Zeus.Security;

namespace Zeus{

    public class SpecTest{

        SecurityAttributes securityAttributes;

        public SpecTest(){
            this.securityAttributes = new SecurityAttributes("stargz.r", "secured");
        }

        public void A(){
            ViewCache viewCache = this.create();
            ExperienceResolver exp = new ExperienceResolver();
            StringBuilder sb = new StringBuilder();
            sb.Append("<c:if spec=\"${tom.name == ''}\">\n");
            sb.Append("    *!.\n");
            sb.Append("</c:if>\n");
            sb.Append("<c:foreach items=\"${todos}\" var=\"tdo\">\n");
            sb.Append("     <c:if spec=\"${tdo.title == 'Exercise *0'}\">\n");
            sb.Append("         *ned.\n");
            sb.Append("     </c:if>\n");
            sb.Append("     <c:if spec=\"${tdo.title == 'Exercise *1'}\">\n");
            sb.Append("         *jermaine.\n");
            sb.Append("     </c:if>\n");
            sb.Append("</c:foreach>\n");
            String result = exp.resolve(sb.ToString(), viewCache, null, securityAttributes, new ArrayList());
            String resultFinal = Regex.Replace(result, "([^\\S\\r\\n])+|(?:\\r?\\n)+", "");
            if(!"*ned.*jermaine.".Equals(resultFinal))Console.WriteLine("Fail.");
            if("*ned.*jermaine.".Equals(resultFinal)){
                Console.WriteLine("A Pass!");
            }
        }

        public void B() {
            ViewCache resp = this.create();
            ExperienceResolver exp = new ExperienceResolver();
            StringBuilder sb = new StringBuilder();
            sb.Append("<c:if spec=\"${message != ''}\">\n");
            sb.Append("${message}\n");
            sb.Append("</c:if>\n");
            String result = exp.resolve(sb.ToString(), resp, null, securityAttributes, new ArrayList()).Trim();
            if(!"Effort.".Equals(result))Console.WriteLine("Fail.");
            if("Effort.".Equals(result)){
                Console.WriteLine("B Pass!");
            }
        }

        public void C() {
            ViewCache viewCache = this.create();
            StringBuilder sb = new StringBuilder();
            ExperienceResolver exp = new ExperienceResolver();
            sb.Append("<c:if spec=\"${blank != ''}\">\n");
            sb.Append("nothing.\n");
            sb.Append("</c:if>\n");
            String result = exp.resolve(sb.ToString(), viewCache, null, securityAttributes, new ArrayList()).Trim();
            if(!"".Equals(result))Console.WriteLine("Fail.");
            if("".Equals(result)){
                Console.WriteLine("C Pass!");
            }
        }

        public void D() {
            ViewCache viewCache = this.create();
            StringBuilder sb = new StringBuilder();
            ExperienceResolver exp = new ExperienceResolver();
            sb.Append("<c:if spec=\"${tom.name == 'Tom'}\">\n");
            sb.Append("Tom.\n");
            sb.Append("</c:if>\n");
            sb.Append("<c:if spec=\"${tom.wife.name == 'Penelope'}\">\n");
            sb.Append("Penelope.\n");
            sb.Append("</c:if>\n");
            sb.Append("<c:if spec=\"${tom.wife.pet.name == 'Diego'}\">\n");
            sb.Append("Diego.\n");
            sb.Append("</c:if>\n");
            String result = exp.resolve(sb.ToString(), viewCache, null, securityAttributes, new ArrayList());
            String resultFinal = Regex.Replace(result, "([^\\S\\r\\n])+|(?:\\r?\\n)+", "");
            if(!"Tom.Penelope.Diego.".Equals(resultFinal))Console.WriteLine("Fail.");
            if("Tom.Penelope.Diego.".Equals(resultFinal)){
                Console.WriteLine("D Pass!");
            }
        }

        public void E() {
            ViewCache viewCache = this.create();
            StringBuilder sb = new StringBuilder();
            ExperienceResolver exp = new ExperienceResolver();
            sb.Append("<c:if spec=\"${not == null}\">\n");
            sb.Append("not.\n");
            sb.Append("</c:if>\n");
            sb.Append("<c:if spec=\"${not != null}\">\n");
            sb.Append("!not.\n");
            sb.Append("</c:if>\n");
            String result = exp.resolve(sb.ToString(), viewCache, null, securityAttributes, new ArrayList());
            String resultFinal = Regex.Replace(result, "([^\\S\\r\\n])+|(?:\\r?\\n)+", "");
            if(!"not.".Equals(resultFinal))Console.WriteLine("Fail.");
            if("Tom.Penelope.Diego.".Equals(resultFinal)){
                Console.WriteLine("E Pass!");
            }
        }

        public void F() {
            ViewCache viewCache = this.create();
            StringBuilder sb = new StringBuilder();
            ExperienceResolver exp = new ExperienceResolver();
            sb.Append("<c:if spec=\"${todos.Count > 0}\">\n");
            sb.Append(" <c:foreach items=\"${todos}\" var=\"tdo\">\n");
            sb.Append("     <c:set var=\"selected\" val=\"\" \n");
            sb.Append("     <c:if spec=\"${tdo.title == 'Exercise *1'}\">\n");
            sb.Append("         <c:set var=\"selected\" val=\"selected\"\n");
            sb.Append("     </c:if>\n");
            sb.Append("     :${selected}:\n");
            sb.Append("     <c:foreach items=\"${tdo.people}\" var=\"person\">\n");
            sb.Append("         ${tdo.id} -> ${person.pet.name},\n");
            sb.Append("     </c:foreach>\n");
            sb.Append(" </c:foreach>\n");
            sb.Append("</c:if>\n");
            sb.Append("<c:if spec=\"${todos.Count == 0}\">\n");
            sb.Append("     everyonealright.\n");
            sb.Append("</c:if>\n");
            String result = exp.resolve(sb.ToString(), viewCache, null, securityAttributes, new ArrayList());
            String resultFinal = Regex.Replace(result, "([^\\S\\r\\n])+|(?:\\r?\\n)+", "");
            if(!"::0->Apache*0,0->Apache*1,0->Apache*2,:selected:1->Apache*0,1->Apache*1,1->Apache*2,::2->Apache*0,2->Apache*1,2->Apache*2,".Equals(resultFinal))Console.WriteLine("Fail.");
            if("::0->Apache*0,0->Apache*1,0->Apache*2,:selected:1->Apache*0,1->Apache*1,1->Apache*2,::2->Apache*0,2->Apache*1,2->Apache*2,".Equals(resultFinal)){
                Console.WriteLine("F Pass!");
            }
        }

        public void G() {
            ViewCache viewCache = this.create();
            StringBuilder sb = new StringBuilder();
            ExperienceResolver exp = new ExperienceResolver();
            sb.Append("<c:if spec=\"${tom.name == 'Tom'}\">\n");
            sb.Append("     <c:if spec=\"${condition}\">\n");
            sb.Append("         condition.\n");
            sb.Append("     </c:if>\n");
            sb.Append("</c:if>\n");
            String result = exp.resolve(sb.ToString(), viewCache, null, securityAttributes, new ArrayList());
            String resultFinal = Regex.Replace(result, "([^\\S\\r\\n])+|(?:\\r?\\n)+", "");
            if(!"condition.".Equals(resultFinal))Console.WriteLine("Fail.");
            if("condition.".Equals(resultFinal)){
                Console.WriteLine("G Pass!");
            }
        }

        public ViewCache create() {

            ViewCache cache = new ViewCache();

            ArrayList todos = new ArrayList();

            Int32 personIdx = 4;
            for(int idx = 0; idx < 3; idx++) {
                Todo todo = new Todo();
                todo.setId(idx);
                todo.setTitle("Exercise *" + idx);

                ArrayList people = new ArrayList();
                for (int idxn = 0; idxn < 3; idxn++) {
                    Person person = new Person();
                    person.setId(personIdx);
                    person.setName("Brad *" + personIdx);personIdx++;
                    Pet pet = new Pet();
                    pet.setId(idxn);
                    pet.setName("Apache *" + idxn);
                    person.setPet(pet);
                    people.Add(person);
                }
                todo.setPeople(people);
                todos.Add(todo);
            }

            Actor topgun = getActor();
            Actor blackjoe = getIt();
            Actor burgandy = getBlankPet();
            Actor iceman = getNilPet();

            cache.set("not", null);
            cache.set("blank", "");
            cache.set("message", "Effort.");
            cache.set("nil", "");
            cache.set("tom", topgun);
            cache.set("brad", blackjoe);
            cache.set("will", burgandy);
            cache.set("val", iceman);
            cache.set("todos", todos);
            cache.set("true", true);
            cache.set("condition", true);

            return cache;
        }

        Actor getIt(){
            Pet pet = new Pet();
            pet.setId(12);
            pet.setName("Agatha");

            Actor wife = new Actor();
            wife.setId(9);
            wife.setName("Jennifer");
            wife.setPet(pet);

            Actor actor = new Actor();
            actor.setId(10);
            actor.setName("Pitt");
            actor.setWife(wife);

            return actor;
        }

        Actor getActor(){
            Pet pet = new Pet();
            pet.setId(3);
            pet.setName("Diego");

            Actor wife = new Actor();
            wife.setId(2);
            wife.setName("Penelope");
            wife.setPet(pet);

            Actor actor = new Actor();
            actor.setId(1);
            actor.setName("Tom");
            actor.setWife(wife);

            return actor;
        }

        Actor getBlankPet(){
            Pet pet = new Pet();
            pet.setId(4);
            pet.setName("");

            Actor wife = new Actor();
            wife.setId(5);
            wife.setName("Mrs. Will");
            wife.setPet(pet);

            Actor actor = new Actor();
            actor.setId(6);
            actor.setName("Mr. Will");
            actor.setWife(wife);

            return actor;
        }

        Actor getNilPet(){

            Actor wife = new Actor();
            wife.setId(7);
            wife.setName("Joanne");

            Actor actor = new Actor();
            actor.setId(8);
            actor.setName("Val");
            actor.setWife(wife);

            return actor;
        }

        public class Actor {
            public Int32 id;
            public String name;
            public Int32 age;
            public Actor wife;
            public Pet pet;

            public Int32 getId()
            {
                return this.id;
            }

            public void setId(Int32 id)
            {
                this.id = id;
            }

            public String getName()
            {
                return this.name;
            }

            public void setName(String name)
            {
                this.name = name;
            }

            public Int32 getAge()
            {
                return this.age;
            }

            public void setAge(Int32 age)
            {
                this.age = age;
            }

            public Actor getWife()
            {
                return this.wife;
            }

            public void setWife(Actor wife)
            {
                this.wife = wife;
            }

            public Pet getPet()
            {
                return this.pet;
            }

            public void setPet(Pet pet)
            {
                this.pet = pet;
            }
        }

        public class Pet {
            public Int32 id;
            public String name;

            public Int32 getId()
            {
                return this.id;
            }

            public void setId(Int32 id)
            {
                this.id = id;
            }

            public String getName()
            {
                return this.name;
            }

            public void setName(String name)
            {
                this.name = name;
            }

        }

        public class Person {
            public Int32 id;
            public Int32 todoId;
            public String name;
            public Pet pet;
            public ArrayList pets;

            public Int32 getId()
            {
                return this.id;
            }

            public void setId(Int32 id)
            {
                this.id = id;
            }

            public Int32 getTodoId()
            {
                return this.todoId;
            }

            public void setTodoId(Int32 todoId)
            {
                this.todoId = todoId;
            }

            public String getName()
            {
                return this.name;
            }

            public void setName(String name)
            {
                this.name = name;
            }

            public Pet getPet()
            {
                return this.pet;
            }

            public void setPet(Pet pet)
            {
                this.pet = pet;
            }

            public ArrayList getPets()
            {
                return this.pets;
            }

            public void setPets(ArrayList pets)
            {
                this.pets = pets;
            }

        }


        public class Todo {

            public Int64 id;
            public String title;
            public Boolean complete;
            public Person person;
            public ArrayList people;

            public Int64 getId()
            {
                return this.id;
            }

            public void setId(Int64 id)
            {
                this.id = id;
            }

            public String getTitle()
            {
                return this.title;
            }

            public void setTitle(String title)
            {
                this.title = title;
            }

            public Boolean getComplete()
            {
                return this.complete;
            }

            public void setComplete(Boolean complete)
            {
                this.complete = complete;
            }

            public Person getPerson()
            {
                return this.person;
            }

            public void setPerson(Person person)
            {
                this.person = person;
            }

            public ArrayList getPeople()
            {
                return this.people;
            }

            public void setPeople(ArrayList people)
            {
                this.people = people;
            }
        }
    }
}
