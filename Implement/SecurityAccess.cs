using System;
using System.Collections.Generic;

namespace Skyline.Implement {

    public interface SecurityAccess {
        /**
        * Intended to return the user's password based
        * on the username
        *
        * @param user
        * @return returns hashed password
        */
        String getPassword(String user);


        /**
        * takes a username
        *
        * @param user
        * @return returns a unique set of role strings
        */
        HashSet<String> getRoles(String user);


        /**
        *
        * @param user
        * @return returns a unique set of user permissions
        * net.plsar.example permission user:maintenance:(id) (id)
        * replaced with actual id of user
        */
        HashSet<String> getPermissions(String user);

        void setDataAccess(DataAccess dao);
    }

}