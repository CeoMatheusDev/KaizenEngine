using System;

namespace Kaizen
{
    /// <summary>
    /// An attribute that excludes a member from registering as a method or property in Kaizen.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class IgnoreMemberAttribute : Attribute { }
}
