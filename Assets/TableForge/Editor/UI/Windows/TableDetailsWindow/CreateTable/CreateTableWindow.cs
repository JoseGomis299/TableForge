namespace TableForge.Editor.UI
{
    internal class CreateTableWindow : TableDetailsWindow<CreateTableViewModel>
    {
        public static void ShowWindow(CreateTableViewModel viewModel)
        {
            ShowWindow<CreateTableWindow>(viewModel, "Create Table");
        }
        
        protected override void OnConfirm()
        {
            viewModel.CreateTable();
        }

        protected override string GetTableName()
        {
            if (string.IsNullOrEmpty(viewModel.TableName) || viewModel.IsDefaultName(viewModel.TableName))
            {
                return viewModel.GetDefaultName();
            }
            
            return viewModel.TableName;
        }
    }
}