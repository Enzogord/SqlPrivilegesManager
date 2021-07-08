using System.Collections.Generic;

namespace SqlPrivilegeManager.MariaDB
{
    public interface IShowGrantsRowParser
    {
        IEnumerable<IGrantedPrivilege> ParseOneRow(string showGrantsRow);
    }
}
