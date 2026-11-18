using System;

#nullable enable

namespace Kaizen
{
    /// <summary>
    /// Exposes the target class as a global script class to Kaizen Engine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlobalClassAttribute : Attribute { }
}
