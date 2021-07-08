using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB
{
    internal static class PrivilegeFunctions
    {
        public static IEnumerable<string> GetAllGlobalPrivileges()
        {
            var privilegeTypes = Enum.GetValues(typeof(GlobalPrivilegeType)).OfType<GlobalPrivilegeType>();
            foreach (var privilegeType in privilegeTypes)
            {
                yield return privilegeType.GetPrivilegeName();
            }
        }

        public static IEnumerable<string> GetAllDatabasePrivileges()
        {
            var privilegeTypes = Enum.GetValues(typeof(DatabasePrivilegeType)).OfType<DatabasePrivilegeType>();
            foreach (var privilegeType in privilegeTypes)
            {
                yield return privilegeType.GetPrivilegeName();
            }
        }

        public static IEnumerable<string> GetAllTablePrivileges()
        {
            var privilegeTypes = Enum.GetValues(typeof(TablePrivilegeType)).OfType<TablePrivilegeType>();
            foreach (var privilegeType in privilegeTypes)
            {
                yield return privilegeType.GetPrivilegeName();
            }
        }

        public static string GetPrivilegeName(this GlobalPrivilegeType privilege)
        {
            return GetPrivilegeNameForAnyType(privilege);
        }

        public static string GetPrivilegeName(this DatabasePrivilegeType privilege)
        {
            return GetPrivilegeNameForAnyType(privilege);
        }

        public static string GetPrivilegeName(this TablePrivilegeType privilege)
        {
            return GetPrivilegeNameForAnyType(privilege);
        }

        private static string GetPrivilegeNameForAnyType(Enum privilegeEnum)
        {
            var type = privilegeEnum.GetType();
            var memberInfo = type.GetMember(privilegeEnum.ToString());
            IEnumerable<object> attributes = memberInfo[0].GetCustomAttributes(typeof(PrivilegeAttribute), false);
            if (attributes.Any())
            {
                var attribute = (PrivilegeAttribute)attributes.First();
                return attribute.PrivilegeName;
            }
            else
            {
                throw new SqlPrivilegeException($"Attribute {nameof(PrivilegeAttribute)} not set for privilege {privilegeEnum} ({type.FullName})");
            }
        }
    }
}
