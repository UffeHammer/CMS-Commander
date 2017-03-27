using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using System.Collections.Specialized;

namespace SitecoreConverter.Core
{
    [Flags]
    public enum AccessRights 
    {
        NotSet = 0x00,
        Read = 0x01, 
        Write = 0x02,
        Rename = 0x04,
        Create = 0x08,
        Delete = 0x10,
        Administer = 0x20,
        DenyRead = 0x40, 
        DenyWrite = 0x80,
        DenyRename = 0x100,
        DenyCreate = 0x200,
        DenyDelete = 0x400,
        DenyAdminister = 0x800,
        AllowAll = 0x1000,
//        Inheritance = 0x2000
    };


    public interface IRole
    {
        // Name is "DomainName/RoleName" or "DomainName/UserName"
        string Name { get; }
        string ID { get; }
        string Path { get; }
        AccessRights AccessRight { get; }
        NameValueCollection UserSettings { get; }
    }
}
