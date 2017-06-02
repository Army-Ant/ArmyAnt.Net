using System;
namespace ArmyAnt.Utility.Json
{
    public class JNull : IUnit
    {
        public static IUnit isThis(string text)
        {
            return text.Trim().Trim(new char[] { '\r', '\n' }) == "null" ? new Undefined() : null;
        }

        public string String
		{
			get
			{
				return "null";
			}
			set
			{
				throw new JException("Cannot set value to null");
			}
		}
		public EType Type
		{
			get
			{
				return EType.Null;
			}
		}

        public IUnit this[string index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


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
