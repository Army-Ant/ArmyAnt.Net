using System;
namespace ArmyAnt.ArmyAnt.Utility.Json
{
	public class JsonString : JsonNull
	{
		public JsonString(string v = "")
			: base()
		{
			value = v;
		}
		public override string String
		{
			get
			{
				return '"' + value + '"';
			}
			set
			{
				hasValue = true;
				var realValue = value.Trim().Trim(new char[] { '\r', '\n' });
				if (realValue[realValue.Length - 1] != '\0')
					realValue += '\0';
				if (realValue[0] != '"' || realValue[realValue.Length - 2] != '"')
				{
					hasValue = false;
					return;
				}
				this.value = realValue.Remove(realValue.Length - 2).Remove(0, 1);
			}
		}
		public override EJsonValueType Type
		{
			get
			{
				return EJsonValueType.String;
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
				this.value = Convert.ToString(value);
			}
		}

		public override string ToString()
		{
			return value;
		}
		private string value = "";
	}

}
