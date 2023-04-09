using System;
using System.Collections;

using Skyline;
using Skyline.Model;
using Skyline.Implement;

using Persistence.Model;
using Persistence.Repo;

namespace Persistence {

    public class AuthAccess : SecurityAccess {
        
        UserRepo userRepo;
        DataTransferObject dto;

        public AuthAccess(){}

        public AuthAccess(DataTransferObject dto){
            this.userRepo = new UserRepo(dto);
        }
        
        public String getPassword(String email){
            User user = userRepo.getEmail(email);
            if(user != null){
                return user.getPassword();
            }
            return "";
        }

        public HashSet<String> getRoles(String email){
            User user = userRepo.getEmail(email);
            return userRepo.getRoles(user);
        }

        public HashSet<String> getPermissions(String email){
            User user = userRepo.getEmail(email);
            return userRepo.getPermissions(user);
        }
    }
}

