using System;
using System.Collections.Generic;
using ArmyAnt.Utility.Json;

namespace ArmyAnt
{
    namespace Utility.Json
    {

        public interface IUnit
        {
            string String
            {
                get;
                set;
            }
            EType Type
            {
                get;
            }
            object Value
            {
                get; set;
            }
            IUnit this[string index] { get; set; }
            bool HasValue();
        }

        public class JsonUndefined : IUnit
        {
            public virtual string String
            {
                get
                {
                    throw new JsonException();
                }
                set
                {
                }
            }
            public virtual EType Type
            {
                get
                {
                    return EType.Undefined;
                }
            }
            public virtual object Value
            {
                get
                {
                    return null;
                }
                set
                {
                    throw new JsonException();
                }
            }

            public virtual IUnit this[string index]
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public virtual bool HasValue() { return false; }
            public static IUnit Create(string value)
            {
                IUnit i = new JsonObject();
                i.String = value;
                if (i.HasValue())
                    return i;
                i = new JsonArray();
                i.String = value;
                if (i.HasValue())
                    return i;
                i = new JsonString();
                i.String = value;
                if (i.HasValue())
                    return i;
                i = new JsonBoolean();
                i.String = value;
                if (i.HasValue())
                    return i;
                i = new JsonNull();
                i.String = value;
                if (i.HasValue())
                    return i;
                i = new JsonNumeric();
                i.String = value;
                if (i.HasValue())
                    return i;
                return null;
            }
            public static string[] CutByComma(string value)
            {
                value = value.Trim().Trim(new char[] { '\r', '\n' });
                var ret = new List<string>();
                var tmp = "";
                bool isInSingleString = false;
                bool isInDoubleString = false;
                int deepInArray = 0;
                int deepInObject = 0;
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == '\'' && !isInDoubleString)
                    {
                        if (isInSingleString)
                        {
                            if (i == 0 || value[i - 1] != '\\')
                                isInSingleString = false;
                        }
                        else
                            isInSingleString = true;
                    }
                    else if (value[i] == '"' && !isInSingleString)
                    {
                        if (isInDoubleString)
                        {
                            if (i == 0 || value[i - 1] != '\\')
                                isInDoubleString = false;
                        }
                        else
                            isInDoubleString = true;
                    }
                    else if (value[i] == '[' && !isInSingleString && !isInDoubleString)
                        deepInArray++;
                    else if (value[i] == '{' && !isInSingleString && !isInDoubleString)
                        deepInObject++;
                    else if (value[i] == ']' && !isInSingleString && !isInDoubleString)
                        deepInArray--;
                    else if (value[i] == '}' && !isInSingleString && !isInDoubleString)
                        deepInObject--;
                    if (deepInArray < 0 || deepInObject < 0)
                        throw new JsonException();
                    if (deepInArray == 0 && deepInObject == 0 && !isInSingleString && !isInDoubleString && value[i] == ',')
                    {
                        ret.Add(tmp);
                        tmp = "";
                        value = value.Remove(0, i + 1).Trim().Trim(new char[] { '\r', '\n' });
                        i = -1;
                    }
                    else
                        tmp += value[i];
                }
                ret.Add(tmp);
                return ret.ToArray();
            }
        }

        public class JsonNull : JsonUndefined
        {
            public override string String
            {
                get
                {
                    return nullvalue;
                }
                set
                {
                    if (value.Trim().Trim(new char[] { '\r', '\n' }) == nullvalue)
                        hasValue = true;
                }
            }
            public override EType Type
            {
                get
                {
                    return EType.Null;
                }
            }
            public override object Value
            {
                get
                {
                    return null;
                }
                set
                {
                    throw new JsonException();
                }
            }
            public override bool HasValue()
            {
                return hasValue;
            }
            protected bool hasValue = false;
            private const string nullvalue = "null";
        }

        public class JsonException : Exception
        {
            public JsonException() : base()
            {
            }
            public JsonException(string message) : base(message)
            {
            }

        }
    }
}
