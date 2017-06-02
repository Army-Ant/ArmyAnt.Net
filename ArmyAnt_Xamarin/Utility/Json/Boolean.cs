using System;
namespace ArmyAnt.ArmyAnt.Utility.Json
{
	public class JsonBoolean : JsonNull
	{
		public JsonBoolean(bool v = false)
			: base()
		{
			value = v;
		}
		public override string String
		{
			get
			{
				return value.ToString().ToLower();
			}
			set
			{
				switch (value.Trim().Trim(new char[] { '\r', '\n' }))
				{
					case "true":
						this.value = true;
						hasValue = true;
						break;
					case "false":
						this.value = false;
						hasValue = true;
						break;
					default:
						hasValue = false;
						break;
				}
			}
		}
		public override EJsonValueType Type
		{
			get
			{
				return EJsonValueType.Boolean;
			}
		}
		public override object Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = Convert.ToBoolean(value);
			}
		}
		private bool value = false;
	}

}
