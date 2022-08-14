using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Util.Core.Helpers.Domain
{
    public class LDAPUtil
    {
        public static string Host { get; private set; }
        public static string BindDN { get; private set; }
        public static string BindPassword { get; private set; }
        public static int Port { get; private set; }
        public static string BaseDC { get; private set; }
        public static string CookieName { get; private set; }

        public static void Register(IConfiguration configuration)
        {
            Host = configuration.GetValue<string>("LDAPServer");
            Port = configuration?.GetValue<int>("LDAPPort") ?? 389;
            BindDN = configuration.GetValue<string>("BindDN");
            BindPassword = configuration.GetValue<string>("BindPassword");
            BaseDC = configuration.GetValue<string>("LDAPBaseDC");
            CookieName = configuration.GetValue<string>("CookieName");
        }



        public static bool Validate(string username, string password)
        {
            try
            {
                using (var conn = new LdapConnection())
                {
                    conn.Connect(Host, Port);
                    conn.Bind(LdapConnection.LdapV3, "liujiabao", "1qaz!QAZ");
                    var entities =
                        conn.Search(BaseDC, LdapConnection.ScopeSub,
                            $"(sAMAccountName={username})",
                            new string[] { "sAMAccountName" }, false);
                    string userDn = null;
                    while (entities.HasMore())
                    {
                        var entity = entities.Next();
                        var account = entity.GetAttribute("sAMAccountName");
                        //If you need to Case insensitive, please modify the below code.
                        if (account != null && account.StringValue == username)
                        {
                            userDn = entity.Dn;
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(userDn)) return false;
                    conn.Bind(userDn, password);
                    // LdapAttribute passwordAttr = new LdapAttribute("userPassword", password);
                    // var compareResult = conn.Compare(userDn, passwordAttr);
                    conn.Disconnect();
                    return true;
                }
            }
            catch (LdapException ex)
            {

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static object GetCurrentUser()
        {
            string directoryPath = "LDAP://"+ Host;

            return "";
        }

    }

}
