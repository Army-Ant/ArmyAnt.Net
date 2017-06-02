using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ArmyAnt.Utility.Json
{
	
	public class JObject : IJsonCollection, IDictionary<string, IUnit>
    {
        public static IUnit isThis(string text)
        {
            try
            {
                var ret = new JObject();
                ret.String = text;
                return ret;
            }
            catch (JException)
            {
                return null;
            }
        }

        public JObject(Dictionary<string, IUnit> v = null)
			: base()
		{
			if (v != null)
				value = v;
		}
		public virtual string String
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
                var realValue = value.Trim().Trim(new char[] { '\r', '\n' });
                if (realValue[realValue.Length - 1] != '\0')
                    realValue += '\0';
                if (realValue[0] != '{' || realValue[realValue.Length - 2] != '}')
                {
                    return;
                }
                realValue = realValue.Remove(realValue.Length - 2).Remove(0, 1);
                realValue = realValue.Trim().Trim(new char[] { '\r', '\n' });
                this.value = new Dictionary<string, IUnit>();
                if (realValue != "")
                {
                    var res = Helper.CutByComma(realValue);
                    for (int i = 0; i < res.Length; i++)
                    {
                        var ins = CutKeyValue(res[i]);
                        this.value[ins.Key] = Helper.Create(ins.Value);
                    }
                }
            }
        }
		public virtual EType Type
		{
			get
			{
				return EType.Object;
			}
		}

		public virtual ICollection<string> Keys
		{
			get
			{
				return value.Keys;
			}
		}

		public virtual ICollection<IUnit> Values
		{
			get
			{
				return value.Values;
			}
		}

		public virtual int Count
		{
			get
			{
				return value.Count;
			}
		}

		public virtual bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public virtual int Length
		{
			get
			{
				return Count;
			}
		}

		public virtual object SyncRoot
		{
			get
			{
				return value;
			}
		}

		public virtual bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

        public virtual IUnit this[string key]
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

		public virtual bool AddChild(IUnit child, string tag)
		{
			if (value.Keys.Contains(tag))
				return false;
			value[tag] = child;
			return true;
		}

		public virtual bool RemoveChild(string tag)
		{
			value.Remove(tag);
			return true;
		}

		public virtual IUnit GetChild(string tag)
		{
			return value[tag];
		}
		private KeyValuePair<string, string> CutKeyValue(string str)
		{
			str = str.Trim().Trim(new char[] { '\r', '\n' });
			char isSingleKey = str[0];
			if (str[0] != '"' && str[0] != '\'')
				throw new Exception();
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
				throw new Exception();
			return new KeyValuePair<string, string>(key, str.Remove(0, 1).Trim().Trim(new char[] { '\r', '\n' }));
		}

		public virtual bool ContainsKey(string key)
		{
			return value.ContainsKey(key);
		}

		public virtual void Add(string key, IUnit value)
		{
			this.value.Add(key, value);
		}

		public virtual bool Remove(string key)
		{
			return value.Remove(key);
		}

		public virtual bool TryGetValue(string key, out IUnit value)
		{
			return this.value.TryGetValue(key, out value);
		}

		public virtual void Add(KeyValuePair<string, IUnit> item)
		{
			value.Add(item.Key, item.Value);
		}

		public virtual void Clear()
		{
			value.Clear();
		}

		public virtual bool Contains(KeyValuePair<string, IUnit> item)
		{
			return value.Contains(item);
		}

		public virtual void CopyTo(KeyValuePair<string, IUnit>[] array, int arrayIndex)
		{
			var keys = value.Keys.ToArray();
			for (var i = 0; i < value.Count; i++)
			{
				array[arrayIndex++] = new KeyValuePair<string, IUnit>(keys[i], value[keys[i]]);
			}
		}

		public virtual bool Remove(KeyValuePair<string, IUnit> item)
		{
			if (value[item.Key] == item.Value)
				return value.Remove(item.Key);
			return false;
		}

		public virtual IEnumerator<KeyValuePair<string, IUnit>> GetEnumerator()
		{
			return value.GetEnumerator();
		}

		public virtual void CopyTo(Array array, int index)
		{
			CopyTo((KeyValuePair<string, IUnit>[])array, index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return value.GetEnumerator();
		}

        public virtual bool ToBool()
        {
            return value.Count > 0;
        }

        public virtual int ToInt()
        {
            return 0;
        }

        public virtual double ToFloat()
        {
            return 0.0;
        }

        public virtual JObject ToObject()
        {
            return this;
        }

        public virtual JArray ToArray()
        {
            return null;
        }

        public override string ToString()
        {
            return null;
        }

        IEnumerator<IUnit> IEnumerable<IUnit>.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        protected Dictionary<string, IUnit> value = new Dictionary<string, IUnit>();
	}

}
