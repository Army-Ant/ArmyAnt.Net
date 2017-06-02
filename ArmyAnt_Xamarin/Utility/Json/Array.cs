using System;
using System.Collections;
using System.Collections.Generic;

namespace ArmyAnt.Utility.Json
{
    public class JArray : JObject, ICollection, IList<IUnit>, ICollection<IUnit>
    {
        public JArray(IUnit[] v = null)
            : base()
        {
            array = v;
        }
        public override string String
        {
            get
            {
                var ret = "[\n";
                for (var i = 0; array != null && i < array.Length - 1; i++)
                {
                    ret += "  " + array[i].String + ",\n";
                }
                if (array != null && array.Length > 0)
                    ret += "  " + array[array.Length - 1].String + "\n]";
                else
                    ret += "\n]";
                return ret;
            }
            set
            {
                var realValue = value.Trim().Trim(new char[] { '\r', '\n' });
                if (realValue[realValue.Length - 1] != '\0')
                    realValue += '\0';
                if (realValue[0] != '[' || realValue[realValue.Length - 2] != ']')
                {
                    return;
                }
                realValue = realValue.Remove(realValue.Length - 2).Remove(0, 1);
                realValue = realValue.Trim().Trim(new char[] { '\r', '\n' });
                if (realValue != "")
                    try
                    {
                        var res = Helper.CutByComma(realValue);
                        var list = new List<IUnit>();
                        for (int i = 0; i < res.Length; i++)
                        {
                            list.Add(Helper.Create(res[i]));
                        }
                        array = list.ToArray();
                    }
                    catch (JException)
                    {
                    }
            }
        }
        public override EType Type
        {
            get
            {
                return EType.Array;
            }
        }

        public override int Length
        {
            get
            {
                return array == null ? 0 : array.Length;
            }
        }

        public override bool IsReadOnly
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

        public override int Count
        {
            get
            {
                return array == null ? 0 : array.Length;
            }
        }

        public override object SyncRoot
        {
            get
            {
                return array == null ? null : array.SyncRoot;
            }
        }

        public override bool IsSynchronized
        {
            get
            {
                return array == null ? false : array.IsSynchronized;
            }
        }

        public IUnit this[int index]
        {
            get
            {
                return array[index];
            }

            set
            {
                this.array[index] = value;
            }
        }

        public override bool AddChild(IUnit child, string tag = null)
        {
            var ret = array.ToList();
            ret.Add(child);
            array = ret.ToJArray();
            return true;
        }

        public override bool RemoveChild(string tag)
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
            var ret = array.ToList();
            ret.RemoveAt(num);
            array = ret.ToJArray();
            return true;
        }

        public override IUnit GetChild(string tag)
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
            return array[num];
        }

        public void Add(IUnit array)
        {
            if (this.array == null)
            {
                this.array = new IUnit[1];
                this.array[0] = array;
            }
            else
            {
                var oldarray = this.array;
                this.array = new IUnit[oldarray.Length + 1];
                for (var i = 0; i < oldarray.Length; i++)
                {
                    this.array[i] = oldarray[i];
                }
                this.array[oldarray.Length] = array;
            }
        }

        public bool Contains(IUnit array)
        {
            return this.array == null ? false : this.array.Contains(array);
        }

        public override void Clear()
        {
            array = null;
        }

        public int IndexOf(IUnit array)
        {
            if (this.array == null)
                return -1;
            for (var i = 0; i < this.array.Length; i++)
            {
                if (this.array[i] == array)
                    return i;
            }
            return -1;
        }

        public void Insert(int index, IUnit array)
        {
            if (this.array == null && index == 0)
            {
                this.array = new IUnit[1];
                this.array[0] = array;
            }
            else if (index > this.array.Length || index + this.array.Length < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "Wrong argument array of \"index\" !");
            }
            else
            {
                if (index < 0)
                    index += this.array.Length;
                var oldarray = this.array;
                this.array = new IUnit[oldarray.Length + 1];
                for (var i = 0; i < index; i++)
                {
                    this.array[i] = oldarray[i];
                }
                this.array[index] = (IUnit)array;
                for (var i = index; i < oldarray.Length; i++)
                {
                    this.array[i + 1] = oldarray[i];
                }
            }
        }

        public bool Remove(IUnit array)
        {
            var index = IndexOf(array);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            if (array == null || index > array.Length || index + array.Length < 0)
            {
                throw new ArgumentOutOfRangeException("index", index, "Wrong argument array of \"index\" !");
            }
            else
            {
                if (index < 0)
                    index += array.Length;
                var oldarray = array;
                this.array = new IUnit[oldarray.Length - 1];
                for (var i = 0; i < index; i++)
                {
                    array[i] = oldarray[i];
                }
                for (var i = index; i < oldarray.Length - 1; i++)
                {
                    array[i] = oldarray[i + 1];
                }
            }
        }

        public void CopyTo(IUnit[] array, int index)
        {
            array.CopyTo(array, index);
        }

        IEnumerator<IUnit> IEnumerable<IUnit>.GetEnumerator()
        {
            return (IEnumerator<IUnit>)array.GetEnumerator();
        }

        public override IEnumerator GetEnumerator()
        {
            return array.GetEnumerator();
        }

        public override JArray ToArray()
        {
            return this;
        }

        private IUnit[] array = null;
    }

}
