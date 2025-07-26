using System;
using TableForge.Editor.UI;
using TableForge.Editor.UI.UssClasses;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TableForge.Editor
{
    internal class ItemReviewItem : VisualElement
    {
        private readonly ImportViewModel _viewModel;
        private ImportItem _boundItem;

        public TextField PathField { get; }
        public ObjectField ObjectField { get; }
        public Label StatusLabel { get; }

        public ItemReviewItem(ImportViewModel viewModel)
        {
            _viewModel = viewModel;
            AddToClassList(ImportUss.ItemReviewItem);

            // Path row
            var pathRow = new VisualElement();
            pathRow.AddToClassList(ImportUss.ItemReviewItemRow);
            Add(pathRow);

            var pathLabel = new Label("Path:");
            pathLabel.AddToClassList(ImportUss.ItemReviewItemLabel);
            pathRow.Add(pathLabel);

            PathField = new TextField();
            PathField.AddToClassList(ImportUss.ItemReviewItemPathField);
            pathRow.Add(PathField);

            // Asset row
            var assetRow = new VisualElement();
            assetRow.AddToClassList(ImportUss.ItemReviewItemRow);
            Add(assetRow);

            var assetLabel = new Label("Asset:");
            assetLabel.AddToClassList(ImportUss.ItemReviewItemLabel);
            assetRow.Add(assetLabel);

            ObjectField = new ObjectField
            {
                allowSceneObjects = false
            };
            ObjectField.AddToClassList(ImportUss.ItemReviewItemObjectField);
            assetRow.Add(ObjectField);

            StatusLabel = new Label();
            StatusLabel.AddToClassList(ImportUss.ItemReviewItemStatusLabel);
            assetRow.Add(StatusLabel);

            // Register callbacks
            PathField.RegisterValueChangedCallback(OnPathChanged);
            ObjectField.RegisterValueChangedCallback(OnObjectFieldChanged);
        }

        public void Bind(ImportItem item, Type assetType)
        {
            _boundItem = item;
            ObjectField.objectType = assetType;

            PathField.SetValueWithoutNotify(item.Path);
            ObjectField.SetValueWithoutNotify(item.ExistingAsset);
            UpdateStatusLabel();
        }

        private void OnPathChanged(ChangeEvent<string> evt)
        {
            _boundItem.Path = evt.newValue;
            if (PathUtil.TryLoadAsset(evt.newValue, out var asset))
            {
                _boundItem.Guid = AssetDatabase.AssetPathToGUID(evt.newValue);
                _boundItem.ExistingAsset = asset as ScriptableObject;
                ObjectField.SetValueWithoutNotify(_boundItem.ExistingAsset);
            }
            else
            {
                _boundItem.Guid = string.Empty;
                _boundItem.ExistingAsset = null;
                ObjectField.SetValueWithoutNotify(null);
            }
            UpdateStatusLabel();
        }

        private void OnObjectFieldChanged(ChangeEvent<Object> evt)
        {
            _boundItem.ExistingAsset = (ScriptableObject)evt.newValue;
            _boundItem.Guid = evt.newValue != null
                ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(evt.newValue))
                : string.Empty;
            _boundItem.Path = evt.newValue != null 
                ? AssetDatabase.GetAssetPath(evt.newValue) 
                : _boundItem.OriginalPath;

            PathField.SetValueWithoutNotify(_boundItem.Path);
            _viewModel.ValidateItems();
            UpdateStatusLabel();
        }

        private void UpdateStatusLabel()
        {
            bool isValidPath = PathUtil.IsValidPath(_boundItem.Path, ".asset");
            if (isValidPath)
            {
                try
                {
                    _viewModel.ValidateItems();
                }
                catch (Exception)
                {
                    isValidPath = false;
                }
            }

            StatusLabel.text = _boundItem.WillCreateNew 
                ? isValidPath ? "New Asset" : "Invalid Path" 
                : "Existing Asset";

            StatusLabel.RemoveFromClassList(ImportUss.ItemReviewItemStatusLabelNew);
            StatusLabel.RemoveFromClassList(ImportUss.ItemReviewItemStatusLabelExisting);
            StatusLabel.RemoveFromClassList(ImportUss.ItemReviewItemStatusLabelInvalid);

            StatusLabel.AddToClassList(_boundItem.WillCreateNew
                ? isValidPath
                    ? ImportUss.ItemReviewItemStatusLabelNew
                    : ImportUss.ItemReviewItemStatusLabelInvalid
                : ImportUss.ItemReviewItemStatusLabelExisting);
        }
    }
}
