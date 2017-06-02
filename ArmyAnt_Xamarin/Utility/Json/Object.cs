using System;
namespace ArmyAnt.ArmyAnt.Utility.Json
{
	
	public class JsonObject : JsonNull, IJsonCollection, IDictionary<string, IUnit>
	{
		public JsonObject(Dictionary<string, IUnit> v = null)
			: base()
		{
			if (v != null)
				value = v;
		}
		public override string String
		{
			get
			{
				var ret = "{\n";
				var keys = value.Keys.ToArray();
				for (var i = 0; keys != null && i < keys.Length - 1; i++)
				{
					ret += "  \"" + keys[i] + '"' + " : " + value[keys[i]].String + ",\n";
				}
				if (keys != null && keys.Length > 0)
					ret += "  \"" + keys[keys.Length - 1] + '"' + " : " + value[keys[keys.Length - 1]].String + "\n}";
				else
					ret += "\n}";
				return ret;
			}
			set
			{
				hasValue = true;
				var realValue = value.Trim().Trim(new char[] { '\r', '\n' });
				if (realValue[realValue.Length - 1] != '\0')
					realValue += '\0';
				if (realValue[0] != '{' || realValue[realValue.Length - 2] != '}')
				{
					hasValue = false;
					return;
				}
				realValue = realValue.Remove(realValue.Length - 2).Remove(0, 1);
				realValue = realValue.Trim().Trim(new char[] { '\r', '\n' });
				this.value = new Dictionary<string, IUnit>();
				if (realValue != "")
					try
					{
						var res = CutByComma(realValue);
						for (int i = 0; i < res.Length; i++)
						{
							var ins = CutKeyValue(res[i]);
							this.value[ins.Key] = Create(ins.Value);
						}
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
				return EJsonValueType.Object;
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
				this.value = (Dictionary<string, IUnit>)value;
			}
		}

		public ICollection<string> Keys
		{
			get
			{
				return value.Keys;
			}
		}

		public ICollection<IUnit> Values
		{
			get
			{
				return value.Values;
			}
		}

		public int Count
		{
			get
			{
				return value.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public int Length
		{
			get
			{
				return value.Count;
			}
		}

		public object SyncRoot
		{
			get
			{
				return this.value;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public override IUnit this[string key]
		{
			get
			{
				if (value.ContainsKey(key))
					return value[key];
				return null;
			}

			set
			{
				this.value[key] = value;
			}
		}

		public bool AddChild(IUnit child, string tag)
		{
			if (value.Keys.Contains(tag))
				return false;
			value[tag] = child;
			return true;
		}

		public bool RemoveChild(string tag)
		{
			value.Remove(tag);
			return true;
		}

		public IUnit GetChild(string tag)
		{
			return value[tag];
		}
		private KeyValuePair<string, string> CutKeyValue(string str)
		{
			str = str.Trim().Trim(new char[] { '\r', '\n' });
			char isSingleKey = str[0];
			if (str[0] != '"' && str[0] != '\'')
				throw new JsonException();
			var key = "";
			var count = 1;
			while (count < str.Length)
			{
				if (str[count] == isSingleKey)
					break;
				else
					key += str[count++];
			}
			str = str.Remove(0, count + 1).Trim().Trim(new char[] { '\r', '\n' });
			if (str[0] != ':')
				throw new JsonException();
			return new KeyValuePair<string, string>(key, str.Remove(0, 1).Trim().Trim(new char[] { '\r', '\n' }));
		}

		public bool ContainsKey(string key)
		{
			return value.ContainsKey(key);
		}

		public void Add(string key, IUnit value)
		{
			this.value.Add(key, value);
		}

		public bool Remove(string key)
		{
			return value.Remove(key);
		}

		public bool TryGetValue(string key, out IUnit value)
		{
			return this.value.TryGetValue(key, out value);
		}

		public void Add(KeyValuePair<string, IUnit> item)
		{
			value.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			value.Clear();
		}

		public bool Contains(KeyValuePair<string, IUnit> item)
		{
			return value.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, IUnit>[] array, int arrayIndex)
		{
			var keys = value.Keys.ToArray();
			for (var i = 0; i < value.Count; i++)
			{
				array[arrayIndex++] = new KeyValuePair<string, IUnit>(keys[i], value[keys[i]]);
			}
		}

		public bool Remove(KeyValuePair<string, IUnit> item)
		{
			if (value[item.Key] == item.Value)
				return value.Remove(item.Key);
			return false;
		}

		public IEnumerator<KeyValuePair<string, IUnit>> GetEnumerator()
		{
			return value.GetEnumerator();
		}

		public void CopyTo(Array array, int index)
		{
			CopyTo((KeyValuePair<string, IUnit>[])array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return value.GetEnumerator();
		}

		IEnumerator<IUnit> IEnumerable<IUnit>.GetEnumerator()
		{
			return value.Values.GetEnumerator();
		}

		private Dictionary<string, IUnit> value = new Dictionary<string, IUnit>();
	}

}
