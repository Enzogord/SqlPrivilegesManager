using System;

namespace SqlPrivilegeManager.MariaDB
{
    public class PrivilegeQueryBuilder : IPrivilegeQueryBuilder
    {
        public string GetGlobalPrivilegeQuery(PrivilegeOperation operation, string privilege, IGrantee grantee)
        {
            if (string.IsNullOrWhiteSpace(privilege))
            {
                throw new ArgumentException($"'{nameof(privilege)}' cannot be null or empty.", nameof(privilege));
            }

            if(grantee is null)
            {
                throw new ArgumentNullException(nameof(grantee));
            }

            if (operation == PrivilegeOperation.Grant)
            {
                return $"GRANT {privilege} ON *.* TO {GetGranteeQueryPart(grantee)};";
            }
            else
            {
                return $"REVOKE {privilege} ON *.* FROM {GetGranteeQueryPart(grantee)};";
            }
        }

        public string GetDatabasePrivilegeQuery(PrivilegeOperation operation, string privilege, string database, IGrantee grantee)
        {
            if (string.IsNullOrWhiteSpace(privilege))
            {
                throw new ArgumentException($"'{nameof(privilege)}' cannot be null or empty.", nameof(privilege));
            }

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentException($"'{nameof(database)}' cannot be null or empty.", nameof(database));
            }

            if(grantee is null)
            {
                throw new ArgumentNullException(nameof(grantee));
            }

            if (operation == PrivilegeOperation.Grant)
            {
                return $"GRANT {privilege} ON `{database}`.* TO {GetGranteeQueryPart(grantee)};";
            }
            else
            {
                return $"REVOKE {privilege} ON `{database}`.* FROM {GetGranteeQueryPart(grantee)};";
            }
        }

        public string GetTablePrivilegeQuery(PrivilegeOperation operation, string privilege, string database, string table, IGrantee grantee)
        {
            if (string.IsNullOrWhiteSpace(privilege))
            {
                throw new ArgumentException($"'{nameof(privilege)}' cannot be null or empty.", nameof(privilege));
            }

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentException($"'{nameof(database)}' cannot be null or empty.", nameof(database));
            }

            if (string.IsNullOrWhiteSpace(table))
            {
                throw new ArgumentException($"'{nameof(table)}' cannot be null or whitespace.", nameof(table));
            }

            if(grantee is null)
            {
                throw new ArgumentNullException(nameof(grantee));
            }

            if (operation == PrivilegeOperation.Grant)
            {
                return $"GRANT {privilege} ON `{database}`.`{table}` TO {GetGranteeQueryPart(grantee)};";
            }
            else
            {
                return $"REVOKE {privilege} ON `{database}`.`{table}` FROM {GetGranteeQueryPart(grantee)};";
            }
        }

        private string GetGranteeQueryPart(IGrantee grantee)
        {
            if(grantee.IsRole)
            {
                return $"`{grantee.Name}`";
            }
            else
            {
                return $"`{grantee.Name}`@`{grantee.Host}`";
            }
        }
    }
}
