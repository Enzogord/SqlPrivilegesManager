using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB
{
    public class ShowGrantsRowParser : IShowGrantsRowParser
    {
        private const string grantKey = "GRANT ";
        private const string onKey = " ON ";
        private const string toKey = " TO ";

        public IEnumerable<IGrantedPrivilege> ParseOneRow(string showGrantsRow)
        {
            if(string.IsNullOrWhiteSpace(showGrantsRow))
            {
                throw new SqlPrivilegeException($"Cannot parse SHOW GRANTS query. Query: \"{showGrantsRow}\"");
            }

            int grantIndex = grantKey.Length;
            int onIndexStart = showGrantsRow.IndexOf(onKey);

            if(onIndexStart < 0)
            {
                throw new SqlPrivilegeException($"Cannot parse SHOW GRANTS query. Block \"ON\" not found. Query: \"{showGrantsRow}\"");
            }

            int onIndexEnd = onIndexStart + onKey.Length;
            int toIndexStart = showGrantsRow.IndexOf(toKey);

            if(toIndexStart < 0)
            {
                throw new SqlPrivilegeException($"Cannot parse SHOW GRANTS query. Block \"TO\" not found. Query: \"{showGrantsRow}\"");
            }

            int toIndexEnd = toIndexStart + toKey.Length;

            string blockGRANTS = showGrantsRow.Substring(grantIndex, onIndexStart - grantIndex);
            string blockON = showGrantsRow.Substring(onIndexEnd, toIndexStart - onIndexEnd);
            string blockTO = showGrantsRow.Substring(toIndexEnd);
            bool hasGrantOption = showGrantsRow.Contains("WITH GRANT OPTION");

            var databaseAndTable = ParseBlockON(blockON);
            var database = databaseAndTable.Database;
            var table = databaseAndTable.Table;

            var grantee = ParseBlockTO(blockTO);

            var grants = ParseBlockGRANTS(blockGRANTS, database, table).ToList();
            if(!grants.Contains("GRANT OPTION") && hasGrantOption)
            {
                grants.Add("GRANT OPTION");
            }

            List<IGrantedPrivilege> results = new List<IGrantedPrivilege>();
            foreach(var grant in grants)
            {
                var privilege = CreatePrivilege(grant, database, table);
                var result = new GrantedPrivilege(grantee, privilege);
                results.Add(result);
            }

            return results;
        }

        private (string Database, string Table) ParseBlockON(string blockON)
        {
            if(string.IsNullOrWhiteSpace(blockON))
            {
                throw new SqlPrivilegeException($"Cannot parse database and table part of SHOW GRANTS query. Parsed part of query: \"{blockON}\"");
            }

            blockON = blockON.Trim();

            string database = "";
            string table = "";

            if(blockON == "*.*")
            {
                database = "*";
                table = "*";
                return (database, table);
            }

            int quoteCounter = 0;
            char[] acceptedQuotes = new[] { '`' };
            bool hasSeparator = false;

            foreach(var c in blockON)
            {
                if(quoteCounter == 2 && c == '*')
                {
                    table = "*";
                    break;
                }

                if(!hasSeparator && c == '.')
                {
                    hasSeparator = true;
                }

                if(acceptedQuotes.Contains(c))
                {
                    quoteCounter++;
                    continue;
                }

                if(quoteCounter == 1)
                {
                    database += c;
                }

                if(quoteCounter == 3)
                {
                    table += c;
                }

                if(quoteCounter == 4)
                {
                    break;
                }
            }

            if(!hasSeparator)
            {
                throw new SqlPrivilegeException($"Cannot parse database and table part of SHOW GRANTS query. Missing user-host separator \"@\". Parsed part of query: \"{blockON}\"");
            }

            if(quoteCounter != 4 && database != "*" && table != "*")
            {
                throw new SqlPrivilegeException($"Cannot parse database and table part of SHOW GRANTS query. Parsed part of query: \"{blockON}\"");
            }

            if(quoteCounter != 2 && database != "*" && table == "*")
            {
                throw new SqlPrivilegeException($"Cannot parse database and table part of SHOW GRANTS query. Parsed part of query: \"{blockON}\"");
            }

            if(string.IsNullOrWhiteSpace(database) || string.IsNullOrWhiteSpace(table))
            {
                throw new SqlPrivilegeException($"Cannot parse database and table part of SHOW GRANTS query. Parsed part of query: \"{blockON}\"");
            }
            return (database, table);
        }

        private IGrantee ParseBlockTO(string blockTO)
        {
            if(string.IsNullOrWhiteSpace(blockTO))
            {
                throw new SqlPrivilegeException($"Cannot parse user and host part of SHOW GRANTS query. Parsed part of query: \"{blockTO}\"");
            }
            char[] acceptedQuotes = new[] { '`', '\'' };

            IGrantee grantee;
            if(TryParseBlockTO(blockTO, '`', out grantee))
            {
                return grantee;
            }

            if(TryParseBlockTO(blockTO, '\'', out grantee))
            {
                return grantee;
            }

            throw new SqlPrivilegeException($"Cannot parse user and host part of SHOW GRANTS query. Parsed part of query: \"{blockTO}\"");
        }

        private bool TryParseBlockTO(string blockTo, char quote, out IGrantee grantee)
        {
            grantee = null;
            var separator = $"{quote}@{quote}";
            var isUser = blockTo.Contains(separator);
            if(isUser && blockTo.Count(x => x == quote) != 4)
            {
                return false;
            }

            if(!isUser && blockTo.Count(x => x == quote) != 2)
            {
                return false;
            }

            if(isUser)
            {
                string user = "";
                string host = "";
                var quoteCounter = 0;
                foreach(var c in blockTo)
                {
                    if(c == quote)
                    {
                        quoteCounter++;
                        continue;
                    }
                    if(quoteCounter == 1)
                    {
                        user += c;
                    }
                    if(quoteCounter == 3)
                    {
                        host += c;
                    }
                }

                grantee = new UserGrantee(user, host);
                return true;
            }
            else
            {
                if(blockTo.StartsWith($"@{quote}") || blockTo.EndsWith($"{quote}@"))
                {
                    return false;
                }

                string role = "";
                var quoteCounter = 0;
                foreach(var c in blockTo)
                {
                    if(c == quote)
                    {
                        quoteCounter++;
                        continue;
                    }
                    if(quoteCounter == 1)
                    {
                        role += c;
                    }
                }

                grantee = new RoleGrantee(role);
                return true;
            }
        }

        private IEnumerable<string> ParseBlockGRANTS(string blockGrants, string database, string table)
        {
            blockGrants = blockGrants.Trim();

            IEnumerable<string> grants = blockGrants.Split(',');
            if(!grants.Any())
            {
                throw new SqlPrivilegeException($"Cannot parse list of grants of SHOW GRANTS query. Parsed part of query: \"{blockGrants}\"");
            }
            var allPrivileges = GetAllPrivileges(database, table);
            if(grants.Contains("ALL PRIVILEGES"))
            {
                grants = allPrivileges.Where(x => x != "USAGE");
            }
            else
            {
                var parsedGrants = grants.Select(x => x.Trim());
                foreach(var parsedGrant in parsedGrants)
                {
                    if(!allPrivileges.Contains(parsedGrant))
                    {
                        throw new SqlPrivilegeException($"Parsed unknown privilege \"{parsedGrant}\". Parsed part of query: \"{blockGrants}\"");
                    }
                }
                grants = parsedGrants;
            }

            return grants;
        }

        private IEnumerable<string> GetAllPrivileges(string database, string table)
        {
            var privilegeType = GetPrivilegeType(database, table);
            switch(privilegeType)
            {
                case PrivilegeType.Global:
                    return PrivilegeFunctions.GetAllGlobalPrivileges();
                case PrivilegeType.Database:
                    return PrivilegeFunctions.GetAllDatabasePrivileges();
                case PrivilegeType.Table:
                    return PrivilegeFunctions.GetAllTablePrivileges();
                default:
                    throw new SqlPrivilegeException($"Unknown privilege type: {privilegeType}");
            }
        }

        private PrivilegeType GetPrivilegeType(string database, string table)
        {
            if(database == "*" && table == "*")
            {
                return PrivilegeType.Global;
            }

            if(table == "*")
            {
                return PrivilegeType.Database;
            }

            return PrivilegeType.Table;
        }
    
        private IPrivilege CreatePrivilege(string privilege, string database, string table)
        {
            var privilegeType = GetPrivilegeType(database, table);
            switch(privilegeType)
            {
                case PrivilegeType.Global:
                    return new GlobalPrivilege(privilege);
                case PrivilegeType.Database:
                    return new DatabasePrivilege(privilege, database);
                case PrivilegeType.Table:
                    return new TablePrivilege(privilege, database, table);
                default:
                    throw new SqlPrivilegeException($"Unknown privilege type: {privilegeType}");
            }
        }
    }
}
