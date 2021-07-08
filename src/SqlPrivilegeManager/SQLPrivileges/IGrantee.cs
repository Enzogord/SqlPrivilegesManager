using System;

namespace SqlPrivilegeManager
{
    public interface IGrantee
    {
        string Name { get; }
        string Host { get; }
        bool IsRole { get; }
    }

    public sealed class UserGrantee : IGrantee
    {
        public string Name { get; }

        public string Host { get; }

        public bool IsRole => false;

        public UserGrantee(string user, string host)
        {
            if(string.IsNullOrWhiteSpace(user))
            {
                throw new ArgumentException($"'{nameof(user)}' cannot be null or whitespace.", nameof(user));
            }

            if(string.IsNullOrWhiteSpace(host))
            {
                throw new ArgumentException($"'{nameof(host)}' cannot be null or whitespace.", nameof(host));
            }

            Name = user;
            Host = host;
        }
    }

    public sealed class RoleGrantee : IGrantee
    {
        public string Name { get; }

        public string Host => "%";

        public bool IsRole => true;

        public RoleGrantee(string role)
        {
            if(string.IsNullOrWhiteSpace(role))
            {
                throw new ArgumentException($"'{nameof(role)}' cannot be null or whitespace.", nameof(role));
            }

            Name = role;
        }
    }
}
