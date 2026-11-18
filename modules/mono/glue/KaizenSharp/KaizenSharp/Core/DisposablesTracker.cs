using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Kaizen.NativeInterop;

#nullable enable

namespace Kaizen
{
    internal static class DisposablesTracker
    {
        [UnmanagedCallersOnly]
        internal static void OnKaizenShuttingDown()
        {
            try
            {
                OnKaizenShuttingDownImpl();
            }
            catch (Exception e)
            {
                ExceptionUtils.LogException(e);
            }
        }

        private static void OnKaizenShuttingDownImpl()
        {
            bool isStdoutVerbose;

            try
            {
                isStdoutVerbose = OS.IsStdOutVerbose();
            }
            catch (ObjectDisposedException)
            {
                // OS singleton already disposed. Maybe OnUnloading was called twice.
                isStdoutVerbose = false;
            }

            if (isStdoutVerbose)
                GD.Print("Unloading: Disposing tracked instances...");

            // Dispose Kaizen Objects first, and only then dispose other disposables
            // like StringName, NodePath, Kaizen.Collections.Array/Dictionary, etc.
            // The Kaizen Object Dispose() method may need any of the later instances.

            foreach (WeakReference<KaizenObject> item in KaizenObjectInstances.Keys)
            {
                if (item.TryGetTarget(out KaizenObject? self))
                    self.Dispose();
            }

            foreach (WeakReference<IDisposable> item in OtherInstances.Keys)
            {
                if (item.TryGetTarget(out IDisposable? self))
                    self.Dispose();
            }

            if (isStdoutVerbose)
                GD.Print("Unloading: Finished disposing tracked instances.");
        }

        private static ConcurrentDictionary<WeakReference<KaizenObject>, byte> KaizenObjectInstances { get; } =
            new();

        private static ConcurrentDictionary<WeakReference<IDisposable>, byte> OtherInstances { get; } =
            new();

        public static WeakReference<KaizenObject> RegisterKaizenObject(KaizenObject kaizenObject)
        {
            var weakReferenceToSelf = new WeakReference<KaizenObject>(kaizenObject);
            KaizenObjectInstances.TryAdd(weakReferenceToSelf, 0);
            return weakReferenceToSelf;
        }

        public static WeakReference<IDisposable> RegisterDisposable(IDisposable disposable)
        {
            var weakReferenceToSelf = new WeakReference<IDisposable>(disposable);
            OtherInstances.TryAdd(weakReferenceToSelf, 0);
            return weakReferenceToSelf;
        }

        public static void UnregisterKaizenObject(KaizenObject kaizenObject, WeakReference<KaizenObject> weakReferenceToSelf)
        {
            if (!KaizenObjectInstances.TryRemove(weakReferenceToSelf, out _))
                throw new ArgumentException("Kaizen Object not registered.", nameof(weakReferenceToSelf));
        }

        public static void UnregisterDisposable(WeakReference<IDisposable> weakReference)
        {
            if (!OtherInstances.TryRemove(weakReference, out _))
                throw new ArgumentException("Disposable not registered.", nameof(weakReference));
        }
    }
}
