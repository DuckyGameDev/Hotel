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
        private static User _currentUser;
        private static Guest _currentGuest;

        public static User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                _currentGuest = value?.Guest;
            }
        }

        public static Guest CurrentGuest
        {
            get => _currentGuest;
            set => _currentGuest = value;
        }
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
