using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ArmyAnt {
    /// <summary>
    /// 一些通用的扩展方法定义
    /// </summary>
    public static class Utilities {

        public static void Swap<T>(this T a, ref T b) {
            (a, b) = (b, a);
        }

        public static (T, T) GetSwap<T>(this T a, T b) {
            return (b, a);
        }

        public static T[] GetArray<T>(this IEnumerable<T> input) {
            if (input == null) {
                return null;
            }
            var ret = new T[input.Count()];
            int index = -1;
            foreach (var i in input) {
                ret[++index] = i;
            }
            return ret;
        }

        public static string[] GetArray<T>(this T input, int start, int end) where T : Enum {
            var ret = new string[end - start + 1];
            for (var i = start; i <= end; ++i) {
                ret[i - start] = input.ToString();
            }
            return ret;
        }

        public static List<T> GetList<T>(this IEnumerable<T> input) {
            if (input == null) {
                return null;
            }
            var ret = new List<T>(input.Count());
            foreach (var i in input) {
                ret.Add(i);
            }
            return ret;
        }

        public static List<string> GetList<T>(T start, T end) where T : Enum {
            int startNum = Convert.ToInt32(start);
            int endNum = Convert.ToInt32(end);
            var ret = new List<string>(endNum - startNum + 1);
            for (var i = startNum; i <= endNum; ++i) {
                ret.Add(((T)Enum.ToObject(default(T).GetType(), i)).ToString());
            }
            return ret;
        }

        public static List<T> GetPropCollection<T>(this IEnumerable collection, string targetName) {
            var ret = new List<T>();
            foreach (var i in collection) {
                var t = i.GetType();
                foreach (var p in t.GetProperties()) {
                    if (p.GetValue(i) is T tar) {
                        ret.Add(tar);
                    } else {
                        throw new System.Reflection.TargetException();
                    }
                }
            }
            return ret;
        }
    }
}
