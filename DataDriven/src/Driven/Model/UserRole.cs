using System;

namespace Driven.Model{

    public class UserRole{
        int id;
        int userId;
        int roleId;

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

        public int getRoleId() {
            return this.roleId;
        }

        public void setRoleId(int roleId) {
            this.roleId = roleId;
        }

    }
}