using System.Collections.Generic;

namespace SqlPrivilegeManager
{
    public interface ISqlExecutor
    {
        void Execute(string sql);
        IEnumerable<string> ExecuteAndGetResult(string sql);
    }
}
