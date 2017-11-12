using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Whiskey.Utility.Class
{
    public class KeyValue<TKey,TValue>
    {
        public TKey Key { get; set; }

        public TValue Value { get; set; }

        public KeyValue() { 
            
        }
        public KeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
    public class Values<TKey, TValue> : KeyValue<TKey, TValue>
    {
        public bool IsDeleted { get; set; }
        public bool IsEnabled { get; set; }
    }
}
