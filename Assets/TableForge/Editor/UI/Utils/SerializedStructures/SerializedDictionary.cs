using System;
using System.Collections.Generic;
using UnityEngine;

namespace TableForge.UI
{
    [Serializable]
    public class SerializedDictionary<K, V> : SerializedDictionary<K, V, K, V>
    {
        public SerializedDictionary() : base() { }
        
        public SerializedDictionary(SerializedDictionary<K, V> dictionary)
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
        public override K SerializeKey(K key) => key;

        /// <summary>
        /// Conversion to serialize a value
        /// </summary>
        /// <param name="val">The value</param>
        /// <returns>The value</returns>
        public override V SerializeValue(V val) => val;

        /// <summary>
        /// Conversion to serialize a key
        /// </summary>
        /// <param name="key">The key to serialize</param>
        /// <returns>The Key that has been serialized</returns>
        public override K DeserializeKey(K key) => key;

        /// <summary>
        /// Conversion to serialize a value
        /// </summary>
        /// <param name="val">The value</param>
        /// <returns>The value</returns>
        public override V DeserializeValue(V val) => val;
    }    
    
    [Serializable]
    public abstract class SerializedDictionary<K, V, SK, SV> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<SK> m_Keys = new List<SK>();

        [SerializeField]
        List<SV> m_Values = new List<SV>();

        /// <summary>
        /// From <see cref="K"/> to <see cref="SK"/>
        /// </summary>
        /// <param name="key">They key in <see cref="K"/></param>
        /// <returns>The key in <see cref="SK"/></returns>
        public abstract SK SerializeKey(K key);

        /// <summary>
        /// From <see cref="V"/> to <see cref="SV"/>
        /// </summary>
        /// <param name="value">The value in <see cref="V"/></param>
        /// <returns>The value in <see cref="SV"/></returns>
        public abstract SV SerializeValue(V value);


        /// <summary>
        /// From <see cref="SK"/> to <see cref="K"/>
        /// </summary>
        /// <param name="serializedKey">They key in <see cref="SK"/></param>
        /// <returns>The key in <see cref="K"/></returns>
        public abstract K DeserializeKey(SK serializedKey);

        /// <summary>
        /// From <see cref="SV"/> to <see cref="V"/>
        /// </summary>
        /// <param name="serializedValue">The value in <see cref="SV"/></param>
        /// <returns>The value in <see cref="V"/></returns>
        public abstract V DeserializeValue(SV serializedValue);

        /// <summary>
        /// OnBeforeSerialize implementation.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_Keys.Clear();
            m_Values.Clear();

            foreach (var kvp in this)
            {
                m_Keys.Add(SerializeKey(kvp.Key));
                m_Values.Add(SerializeValue(kvp.Value));
            }
        }

        /// <summary>
        /// OnAfterDeserialize implementation.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < m_Keys.Count; i++)
                Add(DeserializeKey(m_Keys[i]), DeserializeValue(m_Values[i]));
        }
    }
}