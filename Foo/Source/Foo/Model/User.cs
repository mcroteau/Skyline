using System;

namespace Foo.Model{
    public class User{
        long id;
        String email;
        String password;

        public long getId() {
            return this.id;
        }

        public void setId(long id) {
            this.id = id;
        }

        public String getEmail() {
            return this.email;
        }

        public void setEmail(String email) {
            this.email = email;
        }

        public String getPassword() {
            if(this.password != null){
                return this.password;
            }
            return "";
        }

        public void setPassword(String password) {
            this.password = password;
        }
    }
}