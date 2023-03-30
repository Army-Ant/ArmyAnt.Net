namespace ArmyAnt
{
    public static class Base
    {
        public static void DebugInfo(string text)
        {
#if UNITY_ENGINE
            UnityEngine.Debug.Log(text);
#else
#endif
        }
        public static void DebugWarning(string text)
        {
#if UNITY_ENGINE
            UnityEngine.Debug.LogWarning(text);
#else
#endif
        }
        public static void DebugAssertion(string text)
        {
#if UNITY_ENGINE
            UnityEngine.Debug.LogAssertion(text);
#else
#endif
        }
        public static void DebugError(string text)
        {
#if UNITY_ENGINE
            UnityEngine.Debug.LogError(text);
#else
#endif
        }
        public static void DebugException(System.Exception exception)
        {
#if UNITY_ENGINE
            UnityEngine.Debug.LogException(exception);
#else
#endif
        }
    }
}
