using System;
using System.Runtime.InteropServices;
using Kaizen.NativeInterop;

namespace Kaizen
{
    public static class Dispatcher
    {
        internal static KaizenTaskScheduler DefaultKaizenTaskScheduler;

        internal static void InitializeDefaultKaizenTaskScheduler()
        {
            DefaultKaizenTaskScheduler?.Dispose();
            DefaultKaizenTaskScheduler = new KaizenTaskScheduler();
        }

        public static KaizenSynchronizationContext SynchronizationContext => DefaultKaizenTaskScheduler.Context;
    }
}
