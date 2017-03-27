using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore.Configuration;
using Sitecore.SecurityModel;
using SitecoreConverter.Core;

namespace ExtendedSitecoreAPI5x
{

    public struct RoleStruct
    {
        public string Name;
        public string ID;
        public string Path;
        public string Email;
        public string FullName;
        public string PassWord;
        public string Roles;
        public bool IsAdmin;
        public AccessRights AccessRight;
    }

    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Security : System.Web.Services.WebService
    {
        private Sitecore.SecurityModel.Credentials _Credentials = null;


        public Sitecore.SecurityModel.Credentials credentials
        {
            get
            {
                return _Credentials;
            }

            set
            {
                value = _Credentials;
            }
        }


        /*
        private void Login(Sitecore.SecurityModel.Credentials credentials)
        {
            
            Error.AssertObject(credentials, "credentials");
            if (Sitecore.Context.IsLoggedIn)
            {
                if (Sitecore.Context.User.Name.Equals(credentials.UserName))
                {
                    return;
                }
                Sitecore.Context.User.Logout();
            }
            Assert.IsTrue(Membership.ValidateUser(credentials.UserName, credentials.Password), "Unknown username or password.");
            UserSwitcher.Enter(Sitecore.Security.Accounts.User.FromName(credentials.UserName, true));
        
        }
        */


        private Sitecore.Data.Items.Item GetItemFromMasterdatabase(string sitecoreID)
        {
            Database database = Factory.GetDatabase("master");
            return database.GetItem(sitecoreID);
        }

        private Sitecore.Data.Items.Item GetItemFromSecuritydatabase(string sitecoreID)
        {
            Database database = Factory.GetDatabase("security");
            return database.GetItem(sitecoreID);
        }

        [WebMethod]
        public string GetItemSecurityString(string sPath)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                Sitecore.Data.Items.Item item = GetItemFromMasterdatabase(sPath);
                for (int i=0; i<item.SecurityField.Assignments.Count; i++)
                {
                    Sitecore.SecurityModel.SecurityAssignment securityAssignment = item.SecurityField.Assignments[i];
                    builder.Append("Domain: " + securityAssignment.DomainName);
                    builder.Append(", group: " + securityAssignment.EntityID.ToString());
                    Sitecore.Data.Items.Item groupItem = GetItemFromSecuritydatabase(securityAssignment.EntityID.ToString());
                    if (groupItem != null)
                        builder.Append("(" + securityAssignment.DomainKey + " - " + groupItem.Name + " - " + groupItem.Paths.ContentPath + ")");
                    
                    builder.Append("\n");
                    Sitecore.SecurityModel.ItemRights rights = securityAssignment.Rights;
                    builder.Append(" - " + rights.ToString());
                    builder.Append("\n");
                }
            }
            return builder.ToString();
        }

        private AccessRights GetConverterAccessRights(Sitecore.SecurityModel.ItemRights rights)
        {
            AccessRights AccessRight = new AccessRights();            
            if ((rights & ItemRights.AllowRead) == ItemRights.AllowRead)
                AccessRight = AccessRight | AccessRights.Read;

            if ((rights & ItemRights.AllowWrite) == ItemRights.AllowWrite)
                AccessRight = AccessRight | AccessRights.Write;

            if ((rights & ItemRights.AllowRename) == ItemRights.AllowRename)
                AccessRight = AccessRight | AccessRights.Rename;

            if ((rights & ItemRights.AllowCreate) == ItemRights.AllowCreate)
                AccessRight = AccessRight | AccessRights.Create;

            if ((rights & ItemRights.AllowDelete) == ItemRights.AllowDelete)
                AccessRight = AccessRight | AccessRights.Delete;

            if ((rights & ItemRights.AllowAdmin) == ItemRights.AllowAdmin)
                AccessRight = AccessRight | AccessRights.Administer;


            if ((rights & ItemRights.DenyRead) == ItemRights.DenyRead)
                AccessRight = AccessRight | AccessRights.DenyRead;

            if ((rights & ItemRights.DenyWrite) == ItemRights.DenyWrite)
                AccessRight = AccessRight | AccessRights.DenyWrite;

            if ((rights & ItemRights.DenyRename) == ItemRights.DenyRename)
                AccessRight = AccessRight | AccessRights.DenyRename;

            if ((rights & ItemRights.DenyCreate) == ItemRights.DenyCreate)
                AccessRight = AccessRight | AccessRights.DenyCreate;

            if ((rights & ItemRights.DenyDelete) == ItemRights.DenyDelete)
                AccessRight = AccessRight | AccessRights.DenyDelete;

            if ((rights & ItemRights.DenyAdmin) == ItemRights.DenyAdmin)
                AccessRight = AccessRight | AccessRights.DenyAdminister;


            if ((rights & ItemRights.AllowAll) == ItemRights.AllowAll)
                AccessRight = AccessRight | AccessRights.AllowAll;

            return AccessRight;
        }
         
        [WebMethod]
        public RoleStruct[] GetItemSecurity(string sPath)
        {
            List<RoleStruct> roles5x = new List<RoleStruct>();
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                Sitecore.Data.Items.Item item = GetItemFromMasterdatabase(sPath);
                for (int i = 0; i < item.SecurityField.Assignments.Count; i++)
                {
                    Sitecore.SecurityModel.SecurityAssignment securityAssignment = item.SecurityField.Assignments[i];
                    Sitecore.Data.Items.Item groupItem = GetItemFromSecuritydatabase(securityAssignment.EntityID.ToString());
                    if (groupItem == null)
                        continue;
                    AccessRights AccessRight = GetConverterAccessRights(securityAssignment.Rights);
                    RoleStruct role = new RoleStruct();

                    role.Name = securityAssignment.DomainName + "\\" + groupItem.Name;
                    role.ID = securityAssignment.EntityID.ToString();
                    role.Path = groupItem.Paths.ContentPath;
                    role.AccessRight = AccessRight;
                    if (groupItem.Fields["email"] != null)
                        role.Email = groupItem.Fields["email"].Value;
                    if (groupItem.Fields["fullname"] != null)
                        role.FullName = groupItem.Fields["fullname"].Value;
                    if (groupItem.Fields["password"] != null)
                        role.PassWord = groupItem.Fields["password"].Value;
                    if ((groupItem.Fields["administrator"] != null) && (groupItem.Fields["administrator"].Value == "1"))
                        role.IsAdmin = true;
                    else
                        role.IsAdmin = false;

                    if (groupItem.Fields["roles"] != null)
                    {
                        role.Roles = "";
                        foreach (string sID in groupItem.Fields["roles"].Value.Split('|'))
                        {
                            Sitecore.Data.Items.Item roleItem = GetItemFromSecuritydatabase(sID);
                            if (roleItem != null)
                            {
                                if (role.Roles != "")
                                    role.Roles += "|";
                                role.Roles += securityAssignment.DomainName + "\\" + roleItem.Name;
                            }
                        }
//                        role.Roles = groupItem.Fields["roles"].Value;
                    }

                    roles5x.Add(role);
                }
            }
            return roles5x.ToArray();
        }


        [WebMethod]
        public RoleStruct[] GetUsers(string sDomain)
        {
            using (new Sitecore.SecurityModel.SecurityDisabler())
            {
                Sitecore.SecurityModel.Domain domain = Sitecore.SecurityModel.Domain.FromName(sDomain);
                Sitecore.Data.Database db = domain.Database;
                if (db == null)
                {
                    return null;
                }
                // create a query that will handle the Role retrieval
                // here we simply rely on the Role's Template ID
                string query = "//*[@@templateid='" + Sitecore.TemplateIDs.User + "']";

                Sitecore.Data.Items.Item[] items = db.SelectItems(query);
                Sitecore.SecurityModel.UserItem[] users = new Sitecore.SecurityModel.UserItem[items.Length];

                List<RoleStruct> roles5x = new List<RoleStruct>();
                // fill this array with Role Items
                for (int i = 0; i < users.Length; i++)
                {
                    users[i] = new Sitecore.SecurityModel.UserItem(items[i], domain);
                    RoleStruct role = new RoleStruct();
                    role.Name = users[i].Domain.Name + "\\" + users[i].Name;
                    role.ID = users[i].ID.ToString();
                    role.Path = users[i].InnerItem.Paths.ContentPath;
                    role.AccessRight = AccessRights.NotSet;
                    role.Email = users[i].Email;
                    role.FullName = users[i].Fullname;
                    if (users[i].InnerItem.Fields["password"] != null)
                        role.PassWord = users[i].InnerItem.Fields["password"].Value;
                    role.IsAdmin = users[i].IsAdministrator;
                    role.Roles = "";
                    for (int t=0; t<users[i].Roles.RoleNames.Length; t++)
                    {                        
                        if (role.Roles != "")
                            role.Roles += "|";
                        role.Roles += users[i].Roles.RoleNames[t];
                    }
                    roles5x.Add(role);
                }
                return roles5x.ToArray();
            }
        }

    }
}