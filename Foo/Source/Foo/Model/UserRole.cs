using System;

namespace Foo.Model{
    public class UserRole{
        long id;
        long userId;
        long roleId;

        public long getId() {
            return this.id;
        }

        public void setId(long id) {
            this.id = id;
        }

        public long getUserId() {
            return this.userId;
        }

        public void setUserId(long userId) {
            this.userId = userId;
        }

        public long getRoleId() {
            return this.roleId;
        }

        public void setRoleId(long roleId) {
            this.roleId = roleId;
        }

    }
}