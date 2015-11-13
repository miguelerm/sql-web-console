using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SqlWebConsole.ApiModels
{
    public class UserProfile
    {
        public string Login { get; set; }

        public string DisplayName { get; set; }

        public string Name { get; set; }

        public string EmailAddress { get; set; }

        public string SamAccountName { get; set; }

        public string OrganizationalUnit { get; set; }
    }
}