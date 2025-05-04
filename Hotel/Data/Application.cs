using Hotel.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel.Data
{
    public static class Application
    {
        public static User? CurrentUser { get; private set; }
        public static Guest? CurrentGuest => CurrentUser?.Guest;
        public static Role? CurrentRole => CurrentUser?.Role;

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsAdmin => CurrentRole?.RoleName == "admin";
        public static bool IsManager => CurrentRole?.RoleName == "manager";
        public static bool IsGuest => CurrentRole?.RoleName == "guest";
    }
}
