using System;
namespace ArmyAnt.ArmyAnt.Utility.Json
{
	public class JsonArray : JsonNull, IJsonCollection, IList<IUnit>, ICollection<IUnit>
	{
		public JsonArray(IUnit[] v = null)
			: base()
		{
			value = v;
		}
		public override string String
		{
			get
			{
				var ret = "[\n";
				for (var i = 0; value != null && i < value.Length - 1; i++)
				{
					ret += "  " + value[i].String + ",\n";
				}
				if (value != null && value.Length > 0)
					ret += "  " + value[value.Length - 1].String + "\n]";
				else
					ret += "\n]";
				return ret;
			}
			set
			{
				hasValue = true;
				var realValue = value.Trim().Trim(new char[] { '\r', '\n' });
				if (realValue[realValue.Length - 1] != '\0')
					realValue += '\0';
				if (realValue[0] != '[' || realValue[realValue.Length - 2] != ']')
				{
					hasValue = false;
					return;
				}
				realValue = realValue.Remove(realValue.Length - 2).Remove(0, 1);
				realValue = realValue.Trim().Trim(new char[] { '\r', '\n' });
				if (realValue != "")
					try
					{
						var res = CutByComma(realValue);
						var list = new List<IUnit>();
						for (int i = 0; i < res.Length; i++)
						{
							list.Add(Create(res[i]));
						}
						this.value = list.ToArray();
					}
					catch (JsonException)
					{
						hasValue = false;
					}
			}
		}
		public override EJsonValueType Type
		{
			get
			{
				return EJsonValueType.Array;
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
				this.value = (IUnit[])value;
			}
		}

		public int Length
		{
			get
			{
				return value == null ? 0 : value.Length;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		public int Count
		{
			get
			{
				return value == null ? 0 : value.Length;
			}
		}

		public object SyncRoot
		{
			get
			{
				return value == null ? null : value.SyncRoot;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return value == null ? false : value.IsSynchronized;
			}
		}

		public IUnit this[int index]
		{
			get
			{
				return value[index];
			}

			set
			{
				this.value[index] = value;
			}
		}

		public bool AddChild(IUnit child, string tag = null)
		{
			var ret = value.ToList();
			ret.Add(child);
			value = ret.ToArray();
			return true;
		}

		public bool RemoveChild(string tag)
		{
			int num;
			try
			{
				num = Convert.ToInt32(tag);
			}
			catch (FormatException)
			{
				return false;
			}
			var ret = value.ToList();
			ret.RemoveAt(num);
			value = ret.ToArray();
			return true;
		}

		public IUnit GetChild(string tag)
		{
			int num;
			try
			{
				num = Convert.ToInt32(tag);
			}
			catch (FormatException)
			{
				return null;
			}
			return value[num];
		}

		public void Add(IUnit value)
		{
			if (this.value == null)
			{
				this.value = new IUnit[1];
				this.value[0] = value;
			}
			else
			{
				var oldvalue = this.value;
				this.value = new IUnit[oldvalue.Length + 1];
				for (var i = 0; i < oldvalue.Length; i++)
				{
					this.value[i] = oldvalue[i];
				}
				this.value[oldvalue.Length] = value;
			}
		}

		public bool Contains(IUnit value)
		{
			return this.value == null ? false : this.value.Contains(value);
		}

		public void Clear()
		{
			value = null;
		}

		public int IndexOf(IUnit value)
		{
			if (this.value == null)
				return -1;
			for (var i = 0; i < this.value.Length; i++)
			{
				if (this.value[i] == value)
					return i;
			}
			return -1;
		}

		public void Insert(int index, IUnit value)
		{
			if (this.value == null && index == 0)
			{
				this.value = new IUnit[1];
				this.value[0] = value;
			}
			else if (index > this.value.Length || index + this.value.Length < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, "Wrong argument value of \"index\" !");
			}
			else
			{
				if (index < 0)
					index += this.value.Length;
				var oldvalue = this.value;
				this.value = new IUnit[oldvalue.Length + 1];
				for (var i = 0; i < index; i++)
				{
					this.value[i] = oldvalue[i];
				}
				this.value[index] = (IUnit)value;
				for (var i = index; i < oldvalue.Length; i++)
				{
					this.value[i + 1] = oldvalue[i];
				}
			}
		}

		public bool Remove(IUnit value)
		{
			var index = IndexOf(value);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			if (value == null || index > value.Length || index + value.Length < 0)
			{
				throw new ArgumentOutOfRangeException("index", index, "Wrong argument value of \"index\" !");
			}
			else
			{
				if (index < 0)
					index += value.Length;
				var oldvalue = value;
				this.value = new IUnit[oldvalue.Length - 1];
				for (var i = 0; i < index; i++)
				{
					value[i] = oldvalue[i];
				}
				for (var i = index; i < oldvalue.Length - 1; i++)
				{
					value[i] = oldvalue[i + 1];
				}
			}
		}

		public void CopyTo(IUnit[] array, int index)
		{
			value.CopyTo(array, index);
		}

		IEnumerator<IUnit> IEnumerable<IUnit>.GetEnumerator()
		{
			return (IEnumerator<IUnit>)value.GetEnumerator();
		}

		public IEnumerator GetEnumerator()
		{
			return value.GetEnumerator();
		}

		private IUnit[] value = null;
	}

}
