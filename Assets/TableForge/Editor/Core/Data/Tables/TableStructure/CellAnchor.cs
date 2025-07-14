namespace TableForge.Editor
{
    /// <summary>
    /// Represents a spreadsheet-style cell anchor (column or row), providing functionality for manipulating cell positions.
    /// </summary>
    internal abstract class CellAnchor
    {
        private string _name;

        /// <summary>
        /// The table to which the cell anchor belongs.
        /// </summary>
        public Table Table { get; }
        
        /// <summary>
        /// The unique identifier of the cell anchor.
        /// </summary>
        /// <remarks>The id is unique only inside its table scope.</remarks>
        public int Id { get; protected set; }

        /// <summary>
        /// The name of the cell anchor.
        /// </summary>
        public virtual string Name
        {
            get => _name;
            protected set => _name = value;
        }

        /// <summary>
        /// The position of the cell anchor in the table in a 1-based index.
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// Indicates whether the cell anchor is static and cannot be moved.
        /// </summary>
        public bool IsStatic { get; set; }
        
        /// <summary>
        /// The position of the cell anchor represented as a string of letters.
        /// <example>
        /// 1 = A, 2 = B, 26 = Z, 27 = AA, 28 = AB, etc.
        /// </example>
        /// </summary>
        public string LetterPosition => PositionUtil.ConvertToLetters(Position);

        protected CellAnchor(string name, int position, Table table)
        {
            Name = name;
            Position = position;
            Table = table;
        }
    }
}