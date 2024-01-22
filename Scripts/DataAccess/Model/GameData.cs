using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = System.Object;

namespace DataAccess.Model
{
    /// <summary>
    /// c# 模拟 lua的表
    /// </summary>
    public class GameData : IEnumerable, IDisposable
    {
        private Dictionary<string, Object> datas;

        public int Count
        {
            get => datas.Count;
        }

        public List<string> Keys
        {
            get => GetKeys();
        }

        public List<Object> Values
        {
            get => GetValues();
        }

        public String Json
        {
            get => this.ToString();
        }


        public GameData()
        {
            datas = new Dictionary<string, Object>();
        }

        public Object this[string key]
        {
            get
            {
                Object result = null;
                datas.TryGetValue(key, out result);
                return result;
            }

            set
            {
                if (datas.ContainsKey(key))
                {
                    datas[key] = value;
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        public KeyValuePair<string, Object> this[int index]
        {
            get => ElementAt(index);
        }

        public void Add(string key, Object value)
        {
            datas.Add(key, value);
        }

        private KeyValuePair<string, Object> ElementAt(int index)
        {
            if (index < 0)
            {
                Debug.LogWarning("index must be greater than zero!!!");
                return new KeyValuePair<string, Object>();
            }

            if (index >= Count)
            {
                Debug.LogWarning("index out of range, please check datas.count");
                return new KeyValuePair<string, Object>();
            }

            int _index = 0;
            using (var e = datas.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (_index == index)
                    {
                        return e.Current;
                    }

                    ++_index;
                }
            }

            Debug.LogWarning("index out of range, please check datas.count");
            return new KeyValuePair<string, Object>();
        }

        public bool HaveKey(string key)
        {
            return datas.ContainsKey(key);
        }

        public bool HaveValue(Object value)
        {
            return datas.ContainsValue(value);
        }

        public void Clear()
        {
            datas.Clear();
        }

        public void Dispose()
        {
            Clear();
            datas = null;
        }

        public override string ToString()
        {
            return string.Empty; //可以使用json相关的转换工具或者自定义json，转为字符串
        }

        public override bool Equals(object obj)
        {
            try
            {
                GameData _compareData = (GameData)obj;
                bool isCountEqual = _compareData.Count == this.Count;
                bool isKeysEqual = _compareData.Keys.Equals(this.Keys);
                bool isValuesEqual = _compareData.Values.Equals(this.Values);
                return isCountEqual && isKeysEqual && isValuesEqual;
            }
            catch
            {
                return false;
            }
        }

        private List<string> GetKeys()
        {
            List<string> keys = new List<string>(datas.Count);
            using (var e = datas.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    keys.Add(e.Current.Key);
                }
            }

            return keys;
        }

        private List<Object> GetValues()
        {
            List<Object> values = new List<Object>(datas.Count);
            using (var e = datas.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    values.Add(e.Current.Key);
                }
            }

            return values;
        }

        public GameDataEnumerator GetEnumerator()
        {
            return new GameDataEnumerator(datas);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class GameDataEnumerator : IEnumerator<KeyValuePair<string, Object>>, System.IDisposable
    {
        private Dictionary<string, Object> dockers;
        private int enumeratorIndex;

        public KeyValuePair<string, object> Current => ElementAt(enumeratorIndex);

        object IEnumerator.Current => throw new NotImplementedException();

        private GameDataEnumerator()
        {
        }

        public GameDataEnumerator(Dictionary<string, Object> _dockers)
        {
            enumeratorIndex = -1;
            this.dockers = _dockers;
        }

        private KeyValuePair<string, Object> ElementAt(int index)
        {
            if (index >= dockers.Count)
            {
                Debug.LogError("index out of range");
                return new KeyValuePair<string, Object>();
            }

            int _tempIndex = 0;
            using (var e = dockers.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    if (_tempIndex == index)
                    {
                        return e.Current;
                    }

                    ++_tempIndex;
                }
            }

            return new KeyValuePair<string, Object>();
        }

        public bool MoveNext()
        {
            ++enumeratorIndex;
            return enumeratorIndex < dockers.Count;
        }

        public void Reset()
        {
            enumeratorIndex = -1;
        }

        public void Dispose()
        {
        }
    }
}