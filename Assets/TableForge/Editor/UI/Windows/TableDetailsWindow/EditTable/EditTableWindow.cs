namespace TableForge.Editor.UI
{
    internal class EditTableWindow : TableDetailsWindow<EditTableViewModel>
    {
        public static void ShowWindow(EditTableViewModel viewModel)
        {
           ShowWindow<EditTableWindow>(viewModel, "Edit Table");
        }
        
        protected override void OnConfirm()
        {
            viewModel.UpdateTable();
        }

        protected override string GetTableName()
        {
            return viewModel.TableName;
        }
    }
}