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
            bool ToBool();
            int ToInt();
            float ToFloat();
            string ToString { get; }

            JObject ToObject();
            JArray ToArray();
        }

        public class Helper
		{
			public static IUnit Create(string value)
			{
				// TODO: redisign it
				return null;
			}

            internal static string[] CutByComma(string value)
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
                        throw new JException();
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

        public class JException : System.Exception
        {
            public JException() : base()
            {
            }
            public JException(string message) : base(message)
            {
            }

        }
    }
}
