using System;

namespace ArmyAnt.IO {
    public class ArgumentTextNotAllowException : ArgumentException {
        public ArgumentTextNotAllowException(string paramName, string value) : base("The parameter \"" + paramName + "\" cannot be value \"" + value + "\"", paramName) {
        }
    }
}
