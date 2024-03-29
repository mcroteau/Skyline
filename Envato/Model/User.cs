using System;

namespace Model {

    public class User{
        public int id;
        public String email;
        public String password;

        public int getId() {
            return this.id;
        }

        public void setId(int id) {
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