﻿using System;
using KarmanNet.Logging;

namespace KarmanNet.Protocol {
    public class SafeInvoker {
        private static void LogExceptionError(Logger log, string name, Exception ex) {
            log.Error("An invocation of the {0} has thrown a(n) {1}: {2}", name, ex.GetType().Name, ex.Message);
        }

        public static bool Invoke(Logger log, Action action) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action)?.Invoke();
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, action.Method.Name, ex);
                    }
                }
            }
            return allSucceeded;
        }

        public static bool Invoke<T>(Logger log, Action<T> action, T obj) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action<T>)?.Invoke(obj);
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, action.Method.Name, ex);
                    }
                }
            }
            return allSucceeded;
        }

        public static bool Invoke<T1, T2>(Logger log, Action<T1, T2> action, T1 obj1, T2 obj2) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action<T1, T2>)?.Invoke(obj1, obj2);
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, action.Method.Name, ex);
                    }
                }
            }
            return allSucceeded;
        }

        public static bool Invoke<T1, T2, T3>(Logger log, Action<T1, T2, T3> action, T1 obj1, T2 obj2, T3 obj3) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action<T1, T2, T3>)?.Invoke(obj1, obj2, obj3);
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, action.Method.Name, ex);
                    }
                }
            }
            return allSucceeded;
        }

        public static bool Invoke<T1, T2, T3, T4>(Logger log, Action<T1, T2, T3, T4> action, T1 obj1, T2 obj2, T3 obj3, T4 obj4) {
            bool allSucceeded = true;
            if (action != null) {
                foreach (Delegate invocation in action.GetInvocationList()) {
                    try {
                        (invocation as Action<T1, T2, T3, T4>)?.Invoke(obj1, obj2, obj3, obj4);
                    } catch (Exception ex) {
                        allSucceeded = false;
                        LogExceptionError(log, action.Method.Name, ex);
                    }
                }
            }
            return allSucceeded;
        }
    }
}
