using System;

namespace Foo.Model{
    public class Permission{
        long id;
        long userId;
        String permission;

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

        public String getPermission() {
            return this.permission;
        }

        public void setPermission(String permission) {
            this.permission = permission;
        }

    }
}