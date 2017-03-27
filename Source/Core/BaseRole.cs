using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Specialized;

namespace SitecoreConverter.Core
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://tempuri.org/")]
    public class BaseRole : IRole, ISerializable
    {
        private string _sName = "";
        private string _sID = "";
        private string _sPath = "";
        private AccessRights _AccessRight = new AccessRights();
        private NameValueCollection _UserSettings = new NameValueCollection();


        //Deserialization constructor.
        public BaseRole(SerializationInfo info, StreamingContext ctxt)
        {
            //Get the values from info and assign them to the appropriate properties

            _sName = (string)info.GetValue("Name", typeof(string));
            _sID = (string)info.GetValue("ID", typeof(string));
            _sPath = (string)info.GetValue("Path", typeof(string));
            _AccessRight = (AccessRights)info.GetValue("AccessRight", typeof(AccessRights));
        }
        
        //Serialization function.

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //You can use any custom name for your name-value pair. But make sure you
            // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
            // then you should read the same with "EmployeeId"

            info.AddValue("Name", _sName);
            info.AddValue("ID", _sID);
            info.AddValue("Path", _sPath);
            info.AddValue("AccessRight", _AccessRight);
        }



        public string Name 
        { 
            get
            {
                return _sName;
            }
         }

        public string ID
        {
            get
            {
                return _sID;
            }
        }

        public string Path
        {
            get
            {
                return _sPath;
            }
        }        

        public AccessRights AccessRight
        {
            get
            {
                return _AccessRight;
            }
        }

        public NameValueCollection UserSettings
        {
            get
            {
                return _UserSettings;
            }
        }

        public BaseRole(string sName, string sID, string sPath, AccessRights AccessRights)
        {
            _sName = sName;
            _sID = sID;
            _sPath = sPath;
            _AccessRight = AccessRights;
        }
/*
        public BaseRole()
        {
            _AccessRight = AccessRights.NotSet;
        }
*/
        public static BaseRole CreateBaseRoleFromSitecore43Rights(string sName, string sRights)
        {
            AccessRights AccessRight = new AccessRights();
            if (sRights.IndexOf("a") > -1)
                AccessRight = AccessRight | AccessRights.Administer;
            if (sRights.IndexOf("c") > -1)
                AccessRight = AccessRight | AccessRights.Create;
            if (sRights.IndexOf("d") > -1)
                AccessRight = AccessRight | AccessRights.Delete;
            if (sRights.IndexOf("r") > -1)
                AccessRight = AccessRight | AccessRights.Read;
            if (sRights.IndexOf("n") > -1)
                AccessRight = AccessRight | AccessRights.Rename;
            if (sRights.IndexOf("w") > -1)
                AccessRight = AccessRight | AccessRights.Write;

             // Approve doesn't exist in sitecore 6
//            if (sRights.IndexOf("p") > -1)
//                _AccessRight = _AccessRight | AccessRights.Approve;
             // Publish doesn't exist in sitecore 6
//            if (sRights.IndexOf("u") > -1)
//                _AccessRight = _AccessRight | AccessRights.Publish;
             // None doesn't exist in sitecore 6
//            if (sRights.IndexOf("o") > -1)
//                _AccessRight = _AccessRight | AccessRights.None;

            return new BaseRole(sName, "", "", AccessRight);
        }
    }
}
