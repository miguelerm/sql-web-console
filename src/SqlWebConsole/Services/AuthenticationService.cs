using SqlWebConsole.ApiModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;

namespace SqlWebConsole.Services
{
    public class AuthenticationService
    {
        public UserProfile Login(string login, string password)
        {
            if (string.Compare(ConfigurationManager.AppSettings["SqlConsole.Auth.Type"], "fake", true) == 0)
            {
                return new UserProfile {
                    DisplayName = "Fake User",
                    Login = login,
                    Name = login,
                    EmailAddress= "me@email.com",
                    OrganizationalUnit = "",
                    SamAccountName = login
                };
            }
            else
            {
                return LoginWithActiveDirectory(login, password);
            }
        }



        private UserProfile LoginWithActiveDirectory(string login, string password)
        {
            using (var ctx = new PrincipalContext(ContextType.Domain, ConfigurationManager.AppSettings["SqlConsole.Auth.DefaultDomain"]))
            {
                if (ctx.ValidateCredentials(login, password))
                {
                    try
                    {
                        var user = UserPrincipal.FindByIdentity(ctx, login);

                        return new UserProfile
                        {
                            Login = login,
                            Name = user.Name,
                            EmailAddress = user.EmailAddress,
                            DisplayName = user.DisplayName,
                            SamAccountName = user.SamAccountName,
                            OrganizationalUnit = GetOrganizationalUnit(user)
                        };
                    }
                    catch (Exception)
                    {
                        return null;
                    }


                }
            }

            return null;
        }

        private string GetOrganizationalUnit(UserPrincipal user)
        {
            string result = string.Empty;
            //Getting the directoryEntry's path and spliting with the "," character
            string[] directoryEntryPath = user.DistinguishedName.Split(',');
            //Getting the each items of the array and spliting again with the "=" character
            foreach (var splitedPath in directoryEntryPath)
            {
                string[] eleiments = splitedPath.Split('=');
                //If the 1st element of the array is "OU" string then get the 2dn element
                if (eleiments[0].Trim() == "OU")
                {
                    result = eleiments[1].Trim();
                    break;
                }
            }
            return result;
        }
    }
}