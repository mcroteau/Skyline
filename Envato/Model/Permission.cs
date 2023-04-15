using System;

namespace Model{

    public class Permission{
        int id;
        int userId;
        String permission;

        public int getId() {
            return this.id;
        }

        public void setId(int id) {
            this.id = id;
        }

        public int getUserId() {
            return this.userId;
        }

        public void setUserId(int userId) {
            this.userId = userId;
        }

        public String getPermission() {
            return this.permission;
        }

        public void setPermission(String permission) {
            this.permission = permission;
        }

    }
}