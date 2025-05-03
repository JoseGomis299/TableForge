namespace TableForge.UI
{
    internal class CreateTableWindow : TableDetailsWindow<CreateTableViewModel>
    {
        public static void ShowWindow(CreateTableViewModel viewModel)
        {
            ShowWindow<CreateTableWindow>(viewModel, "Create Table");
        }
        
        protected override void OnConfirm()
        {
            ViewModel.CreateTable();
        }

        protected override string GetTableName()
        {
            if (string.IsNullOrEmpty(ViewModel.TableName) || ViewModel.IsDefaultName(ViewModel.TableName))
            {
                return ViewModel.GetDefaultName();
            }
            
            return ViewModel.TableName;
        }
    }
}