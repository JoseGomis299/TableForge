using TableForge.Editor.Serialization;

namespace TableForge.Editor.Serialization
{
    internal interface ICellSerializer
    {
        /// <summary>
        /// The type of serialization used for the cell's value.
        /// </summary>
        ISerializer ValueSerializer { get; }
        /// <summary>
        /// Serializes the cell's value in a format suitable for storage or transmission.
        /// </summary>
        /// <returns>A string representation of the cell's value.</returns>
        string Serialize(SerializationOptions options);
        /// <summary>
        /// Deserializes the cell's value from a string representation. Giving to the cell a new value.
        /// </summary>
        /// <param name="data">The serialized data.</param>
        void Deserialize(string data, SerializationOptions options);
        /// <summary>
        /// Deserializes the cell's value from a string representation. Giving to the cell a new value.
        /// </summary>
        /// <param name="data">The serialized data.</param>
        /// <returns>Whether the cell has been successfully deserialized or not</returns>
        bool TryDeserialize(string data, SerializationOptions options);
    }
}