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
            ViewModel.UpdateTable();
        }

        protected override string GetTableName()
        {
            return ViewModel.TableName;
        }
    }
}