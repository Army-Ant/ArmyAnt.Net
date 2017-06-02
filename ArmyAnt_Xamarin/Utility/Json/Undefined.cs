using System;
using System.Collections.Generic;

namespace ArmyAnt.Utility.Json
{
	public class Undefined : IUnit
	{
        public static IUnit isThis(string text)
        {
            return text.Trim().Trim(new char[] { '\r', '\n' }) == "undefined" ? new Undefined() : null;
        }

		public virtual string String
		{
			get
			{
			    return "undefined";
			}
			set
			{
                throw new JException("Cannot set value to undefined");
			}
		}

		public virtual EType Type
		{
			get
			{
				return EType.Undefined;
			}
		}

        public bool ToBool()
        {
            return false;
        }

        public int ToInt()
        {
            return 0;
        }

        public double ToFloat()
        {
            return 0.0;
        }

        public JObject ToObject()
        {
            return null;
        }

        public JArray ToArray()
        {
            return null;
		}

        public override string ToString()
        {
            return null;
        }
    }

}
