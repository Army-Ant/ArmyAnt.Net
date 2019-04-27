using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAnt {
    public static class Utilities {
        public static T IsNotNull<T>(T obj) {
            if(obj == null) {
                throw new ArgumentNullException();
            }
            return obj;
        }
    }
}
