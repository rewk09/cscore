using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace com.csutil {

    public class StopwatchV2 : Stopwatch, IDisposable {

        private long managedMemoryAtStart;
        private long managedMemoryAtStop;
        private long memoryAtStart;
        private long memoryAtStop;
        public string methodName;
        public Action onDispose;

        public StopwatchV2([CallerMemberName] string methodName = null) {
            this.methodName = methodName;
            if (onDispose == null) { onDispose = () => Log.MethodDone(this); }
        }

        public long allocatedManagedMemBetweenStartAndStop { get { return managedMemoryAtStop - managedMemoryAtStart; } }
        public long allocatedMemBetweenStartAndStop { get { return memoryAtStop - memoryAtStart; } }

        public StopwatchV2 StartV2() {
            CaptureMemoryAtStart();
            Start();
            return this;
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private void CaptureMemoryAtStart() {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            managedMemoryAtStart = GC.GetTotalMemory(true);
            memoryAtStart = GetCurrentProcessPrivateMemorySize64();
        }

        public static StopwatchV2 StartNewV2([CallerMemberName] string methodName = null) {
            return new StopwatchV2(methodName).StartV2();
        }

        public void StopV2() {
            Stop();
            CaptureMemoryAtStop();
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_FULL_LOGGING")]
        private void CaptureMemoryAtStop() {
            managedMemoryAtStop = GC.GetTotalMemory(true);
            memoryAtStop = GetCurrentProcessPrivateMemorySize64();
        }

        private long GetCurrentProcessPrivateMemorySize64() {
            if (EnvironmentV2.isWebGL) { return 0; }
            using (var p = Process.GetCurrentProcess()) { return p.PrivateMemorySize64; }
        }

        public string GetAllocatedMemBetweenStartAndStop() {
            return "allocated managed mem: " + ByteSizeToString.ByteSizeToReadableString(allocatedManagedMemBetweenStartAndStop)
                + ", allocated mem: " + ByteSizeToString.ByteSizeToReadableString(allocatedMemBetweenStartAndStop);
        }

        public override string ToString() {
            return $"StopWatch '{methodName}' ({ElapsedMilliseconds} ms)";
        }

        public void Dispose() { onDispose?.Invoke(); }

    }

}