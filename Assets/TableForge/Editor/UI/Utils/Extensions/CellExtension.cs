using UnityEngine;

namespace TableForge.UI
{
    internal static class CellExtension
    {
        /// <summary>
        /// Gets the direction from one cell to another in the table hierarchy.
        /// </summary>
        public static Vector2 GetDirectionTo(this Cell from, Cell to, TableMetadata metadata = null)
        {
            if (from == null || to == null)
                return Vector2.zero;
            
            Table commonTable = from.GetNearestCommonTable(to, out from, out to);
            (int col, int row) fromPosition = PositionUtil.GetPosition(from.GetPosition());
            (int col, int row) toPosition = PositionUtil.GetPosition(to.GetPosition());
            
            if (metadata != null && !commonTable.IsSubTable && metadata.IsTransposed)
            {
                fromPosition = (fromPosition.row, fromPosition.col);
                toPosition = (toPosition.row, toPosition.col);
            }

            return new Vector2(Mathf.Clamp(toPosition.col - fromPosition.col, -1, 1), -Mathf.Clamp(toPosition.row - fromPosition.row, -1, 1));
        }
    }
}