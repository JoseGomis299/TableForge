using System;

namespace TableForge
{
    /// <summary>
    /// Represents a spreadsheet-style cell anchor (column or row), providing functionality for manipulating cell positions.
    /// </summary>
    internal class CellAnchor
    {
        /// <summary>
        /// The table to which the cell anchor belongs.
        /// </summary>
        public Table Table { get; set; }
        
        /// <summary>
        /// The unique identifier of the cell anchor.
        /// </summary>
        /// <remarks>The id is unique only inside its table scope.</remarks>
        public int Id { get; }
        
        /// <summary>
        /// The name of the cell anchor.
        /// </summary>
        public string Name { get; }
        
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
        
        public CellAnchor(string name, int position)
        {
            Name = name;
            Position = position;
            
            Id = HashCode.Combine(name, position);
        }
        
    }
}