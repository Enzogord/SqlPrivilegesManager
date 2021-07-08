using System;

namespace SqlPrivilegeManager.MariaDB
{
    public class PrivilegeAttribute : Attribute
    {
        public string PrivilegeName { get; set; }
    }
}
