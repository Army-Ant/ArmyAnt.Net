using System;
namespace ArmyAnt.Utility.Json
{
	public class JBoolean : IUnit
	{
		public JBoolean(bool v = false)
		{
			value = v;
		}

		public string String
		{
			get
			{
				return value.ToString.ToLower();
			}
			set
			{
				switch (value.Trim().Trim(new char[] { '\r', '\n' }))
				{
					case "true":
						this.value = true;
						break;
					case "false":
					default:
						this.value = false;
						break;
				}
			}
		}

		public EType Type
		{
			get
			{
				return EType.Boolean;
			}
		}

        private bool value = false;

        public bool ToBool()
        {
            return value;
        }

        public int ToInt()
        {
            return value ? 1 : 0;
        }

        public float ToFloat()
        {
            return value ? 1.0 : 0.0;
        }

        public JObject ToObject()
        {
            return null;
        }

        public JArray ToArray()
        {
            return null;
        }
    }

}
