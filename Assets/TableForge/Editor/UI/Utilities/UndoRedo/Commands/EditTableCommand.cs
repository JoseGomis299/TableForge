using System;

namespace TableForge.Editor.UI
{
    internal class EditTableCommand : BaseUndoableCommand
    {
        private readonly TableMetadata _oldMetadataCopy;
        private readonly TableMetadata _newMetadataCopy;
        private readonly Action<TableMetadata> _updateAction;

        public EditTableCommand(TableMetadata oldMetadataCopy, TableMetadata newMetadataCopy, Action<TableMetadata> updateAction)
        {
            _oldMetadataCopy = oldMetadataCopy;
            _newMetadataCopy = newMetadataCopy;
            _updateAction = updateAction;
        }

        public override void Execute()
        {
            _updateAction(_newMetadataCopy);
        }

        public override void Undo()
        {
            _updateAction(_oldMetadataCopy);
        }
    }
}