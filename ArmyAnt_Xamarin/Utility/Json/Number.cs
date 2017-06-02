using System;
namespace ArmyAnt.ArmyAnt.Utility.Json
{
	public class JsonNumeric : JsonNull
	{
		public JsonNumeric(byte v = 0)
			: base()
		{
			value = v;
		}
		public JsonNumeric(short v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(ushort v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(int v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(uint v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(long v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(ulong v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(float v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(double v)
			: base()
		{
			value = v;
		}
		public JsonNumeric(decimal v)
			: base()
		{
			value = Convert.ToDouble(v);
		}
		public override string String
		{
			get
			{
				if (value - Convert.ToInt64(value) == 0)
					return Convert.ToInt64(value).ToString();
				return value.ToString();
			}
			set
			{
				try
				{
					this.value = Convert.ToDouble(value);
					hasValue = true;
				}
				catch (FormatException)
				{
					hasValue = false;
				}
			}
		}
		public override EJsonValueType Type
		{
			get
			{
				return EJsonValueType.Numeric;
			}
		}
		public override object Value
		{
			get
			{
				if (value - Convert.ToInt64(value) == 0)
					return Convert.ToInt64(value);
				return value;
			}
			set
			{
				this.value = Convert.ToDouble(value);
			}
		}
		private double value = 0;
	}

}
