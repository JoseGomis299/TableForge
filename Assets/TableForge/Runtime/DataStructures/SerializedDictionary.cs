using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TableForge.DataStructures
{
    [Serializable]
    public class SerializedDictionary<TK, TV> : SerializedDictionary<TK, TV, TK, TV>
    {
        public SerializedDictionary() : base() { }
        
        public SerializedDictionary(SerializedDictionary<TK, TV> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                Add(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Conversion to serialize a key
        /// </summary>
        /// <param name="key">The key to serialize</param>
        /// <returns>The Key that has been serialized</returns>
        public override TK SerializeKey(TK key) => key;

        /// <summary>
        /// Conversion to serialize a value
        /// </summary>
        /// <param name="val">The value</param>
        /// <returns>The value</returns>
        public override TV SerializeValue(TV val) => val;

        /// <summary>
        /// Conversion to serialize a key
        /// </summary>
        /// <param name="key">The key to serialize</param>
        /// <returns>The Key that has been serialized</returns>
        public override TK DeserializeKey(TK key) => key;

        /// <summary>
        /// Conversion to serialize a value
        /// </summary>
        /// <param name="val">The value</param>
        /// <returns>The value</returns>
        public override TV DeserializeValue(TV val) => val;
    }    
    
    [Serializable]
    public abstract class SerializedDictionary<TK, TV, TSk, TSv> : Dictionary<TK, TV>, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("m_Keys")] [SerializeField]
        List<TSk> mKeys = new();

        [FormerlySerializedAs("m_Values")] [SerializeField]
        List<TSv> mValues = new();

        /// <summary>
        /// From <see cref="K"/> to <see cref="SK"/>
        /// </summary>
        /// <param name="key">They key in <see cref="K"/></param>
        /// <returns>The key in <see cref="SK"/></returns>
        public abstract TSk SerializeKey(TK key);

        /// <summary>
        /// From <see cref="V"/> to <see cref="SV"/>
        /// </summary>
        /// <param name="value">The value in <see cref="V"/></param>
        /// <returns>The value in <see cref="SV"/></returns>
        public abstract TSv SerializeValue(TV value);


        /// <summary>
        /// From <see cref="SK"/> to <see cref="K"/>
        /// </summary>
        /// <param name="serializedKey">They key in <see cref="SK"/></param>
        /// <returns>The key in <see cref="K"/></returns>
        public abstract TK DeserializeKey(TSk serializedKey);

        /// <summary>
        /// From <see cref="SV"/> to <see cref="V"/>
        /// </summary>
        /// <param name="serializedValue">The value in <see cref="SV"/></param>
        /// <returns>The value in <see cref="V"/></returns>
        public abstract TV DeserializeValue(TSv serializedValue);

        /// <summary>
        /// OnBeforeSerialize implementation.
        /// </summary>
        public void OnBeforeSerialize()
        {
            mKeys.Clear();
            mValues.Clear();

            foreach (var kvp in this)
            {
                mKeys.Add(SerializeKey(kvp.Key));
                mValues.Add(SerializeValue(kvp.Value));
            }
        }

        /// <summary>
        /// OnAfterDeserialize implementation.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < mKeys.Count; i++)
                Add(DeserializeKey(mKeys[i]), DeserializeValue(mValues[i]));
        }
    }
}