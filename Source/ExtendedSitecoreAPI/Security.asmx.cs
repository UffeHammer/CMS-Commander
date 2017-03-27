using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Diagnostics;
using System.Web.Security;
using Sitecore.Security.Accounts;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using Sitecore.Security.AccessControl;

using Sitecore.Data.Proxies;
using Sitecore.Data.Engines;
using Sitecore.Install.Files;
using Sitecore.Install.Framework;
using Sitecore.Install.Items;

using System.IO;
// using Sitecore.ContentSearch.Maintenance;

namespace ExtendedSitecoreAPI
{
    /// <summary>
    /// Summary description for Security
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
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

        private void Login(Credentials credentials)
        {
            Error.AssertObject(credentials, "credentials");
            if (Sitecore.Context.IsLoggedIn)
            {
                if (Sitecore.Context.User.Name.Equals(credentials.UserName))
                {
                    return;
                }
                Sitecore.Context.Logout();
            }
            Assert.IsTrue(Membership.ValidateUser(credentials.UserName, credentials.Password), "Unknown username or password.");
            UserSwitcher.Enter(Sitecore.Security.Accounts.User.FromName(credentials.UserName, true));
            _Credentials = credentials;
        }


        [WebMethod]
        public bool CreateUser(string sDomainUser, string sPassword, string sEmail, string sFullName, string sRoles, bool bIsAdmin, Credentials credentials)
        {
            if (sDomainUser != "")
            {
                Login(credentials);
                if (!Sitecore.Security.Accounts.User.Exists(sDomainUser))
                {
                    System.Web.Security.Membership.CreateUser(sDomainUser, "dummy", sEmail);
                    Sitecore.Security.Accounts.User newUser = Sitecore.Security.Accounts.User.FromName(sDomainUser, true);
                    newUser.Profile.LegacyPassword = sPassword;
                    newUser.Profile.FullName = sFullName;
                    newUser.Profile.IsAdministrator = bIsAdmin;
                    foreach (string sRole in sRoles.Split('|'))
                    {
                        if (sRole != "")
                        {
                            CreateRole(sRole, credentials);
                            newUser.Roles.Add(Sitecore.Security.Accounts.Role.FromName(sRole));
                        }
                    }
                    newUser.Profile.Save();
                }
            }
            return true;
        }


        /// <summary>
        /// The CreateRole() method creates a role in a domain,
        /// if that role does not already exist.
        /// sDomainRole ("domain\role")
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public bool CreateRole(string sDomainRole, Credentials credentials)
        {
            Login(credentials);
            if (!Sitecore.Security.Accounts.Role.Exists(sDomainRole))
            {
                System.Web.Security.Roles.CreateRole(sDomainRole);
            }
            return true;
        }

        [WebMethod]
        public string[] GetRoles(Credentials credentials)
        {
            Login(credentials);
            Sitecore.Security.SitecoreRoleProvider provider = new Sitecore.Security.SitecoreRoleProvider();            
            Sitecore.Security.Accounts.RoleList roles = Sitecore.Security.Accounts.RoleList.FromNames(provider.GetAllRoles());
            string[] sRoles = new string[roles.Count];
            for (int i = 0; i < roles.Count; i++)
            {
                sRoles[i] = roles[i].Name;
            }
            return sRoles;
        }

        /// <summary>
        /// Make one role inherit from another
        /// </summary>
        [WebMethod]
        public bool AddRoleToRole(string sDomainRole, string sTargetRole, Credentials credentials)
        {
            Login(credentials);

            // If TargetRole doesn't exist, then create it.
            if (!Sitecore.Security.Accounts.Role.Exists(sTargetRole))
            {
                System.Web.Security.Roles.CreateRole(sTargetRole);
            }

            Role domainRole = Sitecore.Security.Accounts.Role.FromName(sDomainRole);
            Role targetRole = Sitecore.Security.Accounts.Role.FromName(sTargetRole);

            if ((domainRole == null) || (targetRole == null))
                return false;

            if (! Sitecore.Security.Accounts.RolesInRolesManager.IsRoleInRole(domainRole, targetRole, true))
                Sitecore.Security.Accounts.RolesInRolesManager.AddRoleToRole(domainRole, targetRole);
            return true;
        }


        /// <summary>
        /// The System.Web.Security.Roles.DeleteRole() method removes all members 
        /// from the role specified by the first parameter, and then removes that role. 
        /// 
        /// Note!  Depending on the number of users, deleting a role can be a long-running operation
        /// </summary>
        /// <param name="sDomainRole"></param>
        /// <returns></returns>
        [WebMethod]
        public bool DeleteRole(string sDomainRole, Credentials credentials)
        {
            Login(credentials);
            if (Sitecore.Security.Accounts.Role.Exists(sDomainRole))
            {
                System.Web.Security.Roles.DeleteRole(sDomainRole);
            }
            return true;
        }

        // Delete mutiple user using wildcard, should be used with care, example: sitecore/*
        [WebMethod]
        public int DeleteUsers(string sDomainUsers, Credentials credentials)
        {
            Login(credentials);
            int iCount = 0;
            MembershipUserCollection users = System.Web.Security.Membership.FindUsersByName(sDomainUsers);
            foreach (MembershipUser user in users)
            {
                if (Sitecore.Security.Accounts.User.Exists(user.UserName))
                {
                    System.Web.Security.Membership.DeleteUser(user.UserName);
                    iCount++;
                }
            }
            return iCount;
        }

        private void SetRight(Sitecore.Data.Items.Item item, Sitecore.Security.Accounts.Account account, Sitecore.Security.AccessControl.AccessRight right, Sitecore.Security.AccessControl.AccessPermission rightState, Sitecore.Security.AccessControl.PropagationType propagationType) 
        { 
            Sitecore.Security.AccessControl.AccessRuleCollection accessRules = item.Security.GetAccessRules();
            if (propagationType == Sitecore.Security.AccessControl.PropagationType.Any) 
            { 
                accessRules.Helper.RemoveExactMatches(account, right); 
            } 
            else 
            { 
                accessRules.Helper.RemoveExactMatches(account, right, propagationType); 
            } 
            if (rightState != Sitecore.Security.AccessControl.AccessPermission.NotSet) 
            { 
                if (propagationType == Sitecore.Security.AccessControl.PropagationType.Any) 
                { 
                    accessRules.Helper.AddAccessPermission(account, right, Sitecore.Security.AccessControl.PropagationType.Entity, rightState); 
                    accessRules.Helper.AddAccessPermission(account, right, Sitecore.Security.AccessControl.PropagationType.Descendants, rightState); 
                } 
                else 
                { 
                    accessRules.Helper.AddAccessPermission(account, right, propagationType, rightState); 
                } 
            } 
            item.Security.SetAccessRules(accessRules); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strDatabase"></param>
        /// <param name="strItem"></param>
        /// <param name="strAccount"></param>
        /// <param name="strRight"></param>
        /// <param name="rightState"></param>
        /// <param name="propagationType"></param>
        /// <param name="credentials"></param>
        [WebMethod]
        public void SetRight(string strDatabase, string strItem, string strAccount, string strRights, Sitecore.Security.AccessControl.AccessPermission rightState, Sitecore.Security.AccessControl.PropagationType propagationType, Credentials credentials)
        {
            Error.AssertString(strDatabase, "strDatabase", false);
            Error.AssertString(strItem, "strItem", false);
            Error.AssertString(strAccount, "strAccount", false);
            Error.AssertString(strRights, "strRights", false);

            Login(credentials);

            Sitecore.Data.Database db = Sitecore.Configuration.Factory.GetDatabase(strDatabase); 
            Sitecore.Data.Items.Item item = db.GetItem(strItem); 
            Sitecore.Security.Accounts.AccountType accountType = Sitecore.Security.Accounts.AccountType.User;
            if (Sitecore.Security.SecurityUtility.IsRole(strAccount)) 
            { 
                accountType = Sitecore.Security.Accounts.AccountType.Role;
            } 
            Sitecore.Security.Accounts.Account account = Sitecore.Security.Accounts.Account.FromName(strAccount, accountType);

            // Always ensure that a minimum of 1 "|" character exists 
            if (strRights.IndexOf("|") == -1)
                strRights += '|';

            string[] strRightsList = strRights.Split('|');
            for (int t = 0; t < strRightsList.Length; t++)
            {
                string strRight = strRightsList[t];
                if ((strRight != null) && (strRight != ""))
                {
                    Sitecore.Security.AccessControl.AccessRight right = Sitecore.Security.AccessControl.AccessRight.FromName(strRight);
                    SetRight(item, account, right, rightState, propagationType);
                }
            }
        }

        [WebMethod]
        public string GetRight(string strDatabase, string strItem, string strAccount, SecurityPermission rightState, Credentials credentials)        
        {            
            Error.AssertString(strDatabase, "strDatabase", false);
            Error.AssertString(strItem, "strItem", false);

            Login(credentials);

            Sitecore.Data.Database db = Sitecore.Configuration.Factory.GetDatabase(strDatabase);
            Sitecore.Data.Items.Item item = db.GetItem(strItem);

            if (strAccount.IndexOf("sitecore\\") == -1)
                strAccount = "sitecore\\" + strAccount;

            Sitecore.Security.Accounts.AccountType accountType = Sitecore.Security.Accounts.AccountType.User;
            if (Sitecore.Security.SecurityUtility.IsRole(strAccount))
            {
                accountType = Sitecore.Security.Accounts.AccountType.Role;
            }
            Sitecore.Security.Accounts.Account account = Sitecore.Security.Accounts.Account.FromName(strAccount, accountType);


            string sResults = "";
            if (rightState == SecurityPermission.AllowAccess)
            {
                if (item.Security.CanAdmin(account))
                    sResults += AccessRight.ItemAdmin + "|";
                if (item.Security.CanCreate(account))
                    sResults += AccessRight.ItemCreate + "|";
                if (item.Security.CanDelete(account))
                    sResults += AccessRight.ItemDelete + "|";
                if (item.Security.CanRead(account))
                    sResults += AccessRight.ItemRead + "|";
                if (item.Security.CanRename(account))
                    sResults += AccessRight.ItemRename + "|";
                if (item.Security.CanWrite(account))
                    sResults += AccessRight.ItemWrite + "|";
            }
            else if (rightState == SecurityPermission.DenyAccess)
            {
                if (!item.Security.CanAdmin(account))
                    sResults += AccessRight.ItemAdmin + "|";
                if (!item.Security.CanCreate(account))
                    sResults += AccessRight.ItemCreate + "|";
                if (!item.Security.CanDelete(account))
                    sResults += AccessRight.ItemDelete + "|";
                if (!item.Security.CanRead(account))
                    sResults += AccessRight.ItemRead + "|";
                if (!item.Security.CanRename(account))
                    sResults += AccessRight.ItemRename + "|";
                if (!item.Security.CanWrite(account))
                    sResults += AccessRight.ItemWrite + "|";
            }
            return sResults;            
        }

        private string GetAccessPermission(AccessRuleCollection rules, Sitecore.Security.Accounts.Account account, AccessRight accessRight, AccessPermission accessPermission, string sExistingPermissions)
        {
            if ((rules.Helper.GetAccessPermission(account, accessRight, PropagationType.Descendants) == accessPermission) &&
                (sExistingPermissions.IndexOf(accessRight.Name) == -1))
            {
                sExistingPermissions += accessRight.Name + "|";
            }
            return sExistingPermissions;
        }




        [WebMethod]
        public string GetAccessRights(Credentials credentials)
        {
            string sResult = "";
            Sitecore.Security.AccessControl.AccessRightCollection col = Sitecore.Security.AccessControl.AccessRightManager.GetAccessRights();
            foreach (Sitecore.Security.AccessControl.AccessRight right in col)
            {
                sResult += right.Name + "|";
            }
            return sResult;
//            Sitecore.Security.AccessControl.AccessRightProvider tmp = Sitecore.Security.AccessControl.AccessRightProvider();
        }

        [WebMethod]
        public string CreateStandardValues(string strItem, Credentials credentials)
        {
            Error.AssertString(strItem, "strItem", false);

            Login(credentials);
            using (new SecurityDisabler())
            {
                // get database for which the template will be created
                Database db = Sitecore.Configuration.Factory.GetDatabase("master");

                Item  standardValuesItem = db.Templates[strItem].CreateStandardValues();
                return standardValuesItem.ID.ToString();
            }

        }


        public MediaItem AddFile(string fileName, string sitecorePath, string mediaItemName)
        {
            // Create the options
            Sitecore.Resources.Media.MediaCreatorOptions options = new Sitecore.Resources.Media.MediaCreatorOptions();
            // Store the file in the database, not as a file
            options.FileBased = false;
            // Remove file extension from item name
            options.IncludeExtensionInItemName = false;
            // Overwrite any existing file with the same name
            options.KeepExisting = false;
            // Do not make a versioned template
            options.Versioned = false;
            // set the path
            options.Destination = sitecorePath + "/" + mediaItemName;
            // Set the database
            options.Database = Sitecore.Configuration.Factory.GetDatabase("master");

            // Now create the file
            Sitecore.Resources.Media.MediaCreator creator = new Sitecore.Resources.Media.MediaCreator();
            MediaItem mediaItem = creator.CreateFromFile(fileName, options);
            return mediaItem;
        }


        [WebMethod]
        public void UploadFile(string FileName, byte[] buffer, long Offset, Credentials credentials)
        {
            Login(credentials);

            // setting the file location to be saved in the server. 
            // reading from the web.config file 
            string FilePath = Path.Combine(Server.MapPath("/temp"), FileName);

            // new file, create an empty file
            if (Offset == 0)
                File.Create(FilePath).Close();
            // open a file stream and write the buffer. 
            // Don't open with FileMode.Append because the transfer may wish to 
            // start a different point
            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                fs.Seek(Offset, SeekOrigin.Begin);
                fs.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// http://www.codeproject.com/Articles/43272/Uploading-Large-Files-Through-Web-Service
        /// This function is used to append chunk of bytes to a file name. 
        /// If the offset starts from 0 means file name should be created.
        /// </summary>
        /// <param name="FileName">File Name</param>
        /// <param name="buffer">Buffer array</param>
        /// <param name="Offset">Offset</param>
        /// <returns>id of newly created item or </returns>
        [WebMethod]
        public string UploadMedia(string FileName, string sitecorePath, byte[] buffer, long Offset, Credentials credentials)
        {
            Login(credentials);

            // setting the file location to be saved in the server. 
            // reading from the web.config file 
            string FilePath = Path.Combine(Server.MapPath("/temp"), FileName);

            // Upload the file
            UploadFile(FileName, buffer, Offset, credentials);

            MediaItem result = AddFile(FilePath, sitecorePath, FileName);
            return result.ID.ToString();
        }


        [WebMethod]
        public void DeleteItem(string sID)
        {
            Login(credentials);

            Database db = Sitecore.Configuration.Factory.GetDatabase("master");
            Item item = db.GetItem(sID);
            EnableIndexing(false);
            try
            {
                item.DeleteChildren();
                item.Delete();
            }
            finally
            {
                EnableIndexing(true);
            }
        }

        /// <summary>
        /// Enable/disable indexing in sitecore
        /// </summary>
        [WebMethod]
        public void EnableIndexing(bool enable)
        {
            // index rebuilding before Sitecore 7 
            Sitecore.Configuration.Settings.Indexing.Enabled = enable;
/*
            // How to pause indexing in Sitecore 7.0 Update-1 (rev130810) and later 
            if (enable == false)
                IndexCustodian.PauseIndexing();
            else
                IndexCustodian.ResumeIndexing();
 */ 
        }



/*	
	  /// <summary>
	  /// Installs a Sitecore Update Package.
	  /// </summary>
	  /// <param name="path">A path to a package that is reachable by the web server</param>
	  [WebMethod(Description = "Installs a Sitecore Update Package.")]
	  public void InstallUpdatePackage(string path)
	  {
	    // Use default logger
	    var log = LogManager.GetLogger("root");
	    XmlConfigurator.Configure((XmlElement)ConfigurationManager.GetSection("log4net"));
	
	    var file = new FileInfo(path);  
	    if (!file.Exists)  
	      throw new ApplicationException(string.Format("Cannot access path '{0}'.", path)); 
	        
	    using (new SecurityDisabler())
	    {
	      var installer = new DiffInstaller(UpgradeAction.Upgrade);
	      var view = UpdateHelper.LoadMetadata(path);
	
	      //Get the package entries
	      bool hasPostAction;
	      string historyPath;
	      var entries = installer.InstallPackage(path, InstallMode.Install, log, out hasPostAction, out historyPath);
	
	      installer.ExecutePostInstallationInstructions(path, historyPath, InstallMode.Install, view, log, ref entries);
	
	      UpdateHelper.SaveInstallationMessages(entries, historyPath);
	    }
	  }
*/		
        /// <summary>
        /// Installs a Sitecore Zip Package.
        /// http://www.sitecore.net/learn/blogs/technical-blogs/mike-skutta/posts/2015/09/sitecore-update-and-zip-package-installer-web-service.aspx
        /// </summary>
        /// <param name="path">A path to a package that is reachable by the web server</param>
        [WebMethod(Description = "Installs a Sitecore Zip Package.")]
        public void InstallZipPackage(string path, Credentials credentials)
        {
            Login(credentials);

            // Fallback to temp folder
            if (File.Exists(Server.MapPath(path)))
                path = Server.MapPath(path);
            else
                path = Server.MapPath("/temp/" + path);
            

	        var file = new FileInfo(path);  
	        if (!file.Exists)  
	            throw new ApplicationException(string.Format("Cannot access path '{0}'.", path)); 
			
	        Sitecore.Context.SetActiveSite("shell");  
	        using (new SecurityDisabler())  
	        {  
	            using (new ProxyDisabler())  
	            {  
	                using (new SyncOperationContext())  
	                {  
	                    IProcessingContext context = new SimpleProcessingContext(); //   
	                    IItemInstallerEvents events =  
	                    new DefaultItemInstallerEvents(new Sitecore.Install.Utils.BehaviourOptions(Sitecore.Install.Utils.InstallMode.Overwrite, Sitecore.Install.Utils.MergeMode.Undefined));  
	                    context.AddAspect(events);  
	                    IFileInstallerEvents events1 = new DefaultFileInstallerEvents(true);  
	                    context.AddAspect(events1);  
	                    var installer = new Sitecore.Install.Installer();  
	                    installer.InstallPackage(path, context);  
	                }  
                }  
	        }  
        }
    }
}
