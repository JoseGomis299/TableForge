# TableForge Reorganization Implementation Script

## Phase 1: Create New Folder Structure

```powershell
# Create new Runtime structure
New-Item -ItemType Directory -Path "Assets/TableForge/Runtime/Core" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Runtime/Core/Attributes" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Runtime/Core/DataStructures" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Runtime/Core/Interfaces" -Force

# Create new Editor structure
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Core" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Core/Attributes" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Core/Exceptions" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Core/Utilities" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Core/Utilities/Extensions" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Cells" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Cells/Base" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Cells/Implementations" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Cells/Implementations/SimpleCells" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Cells/Implementations/SubtableCells" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Cells/Interfaces" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Cells/Factory" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Tables" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Generation" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Generation/ItemSelection" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Generation/SerializedObjects" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Data/Generation/SerializedType" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Serialization" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Serialization/Core" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Serialization/Arguments" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Serialization/Data" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/Serialization/Serializers" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Core" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Core/Attributes" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Core/Enums" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Core/Constants" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Windows" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Windows/AddTabWindow" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Windows/ExportWindow" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Windows/ImportWindow" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Metadata" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Metadata/Data" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Cache" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Cache/Data" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Utilities" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Utilities/Extensions" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Utilities/SizeCalculation" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Utilities/UndoRedo" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Styling" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Styling/UssMappings" -Force
New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Styling/Assets" -Force

New-Item -ItemType Directory -Path "Assets/TableForge/Editor/UI/Data" -Force
```

## Phase 2: Move Runtime Files

```powershell
# Move Runtime files to new structure
Move-Item "Assets/TableForge/Runtime/Attributes/TableForgeIgnoreAttribute.cs" "Assets/TableForge/Runtime/Core/Attributes/"
Move-Item "Assets/TableForge/Runtime/DataStructures/SerializedDictionary.cs" "Assets/TableForge/Runtime/Core/DataStructures/"
```

## Phase 3: Move Editor Core Files

```powershell
# Move Core files
Move-Item "Assets/TableForge/Editor/DataHandling/Attributes/CellTypeAttribute.cs" "Assets/TableForge/Editor/Core/Attributes/"
Move-Item "Assets/TableForge/Editor/DataHandling/Attributes/SubTableCellControlUsageAttribute.cs" "Assets/TableForge/Editor/Core/Attributes/"
Move-Item "Assets/TableForge/Editor/DataHandling/Exceptions/InvalidCellValueException.cs" "Assets/TableForge/Editor/Core/Exceptions/"
Move-Item "Assets/TableForge/Editor/DataHandling/Utils/Extensions/TypeExtension.cs" "Assets/TableForge/Editor/Core/Utilities/Extensions/"
Move-Item "Assets/TableForge/Editor/DataHandling/Utils/CsvParser.cs" "Assets/TableForge/Editor/Core/Utilities/"
```

## Phase 4: Move Data Files

```powershell
# Move Cell files
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/Cell.cs" "Assets/TableForge/Editor/Data/Cells/Base/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/SubTableCell.cs" "Assets/TableForge/Editor/Data/Cells/Base/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/PrimitiveBasedCell.cs" "Assets/TableForge/Editor/Data/Cells/Base/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/INumericBasedCell.cs" "Assets/TableForge/Editor/Data/Cells/Interfaces/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/ISerializableCell.cs" "Assets/TableForge/Editor/Data/Cells/Interfaces/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/IQuotedValueCell.cs" "Assets/TableForge/Editor/Data/Cells/Interfaces/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/CellFactory.cs" "Assets/TableForge/Editor/Data/Cells/Factory/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/SimpleCells/*" "Assets/TableForge/Editor/Data/Cells/Implementations/SimpleCells/"
Move-Item "Assets/TableForge/Editor/DataHandling/Cells/SubtableCells/*" "Assets/TableForge/Editor/Data/Cells/Implementations/SubtableCells/"

# Move Table files
Move-Item "Assets/TableForge/Editor/DataHandling/Table/CellAnchor.cs" "Assets/TableForge/Editor/Data/Tables/"
Move-Item "Assets/TableForge/Editor/DataHandling/Table/Column.cs" "Assets/TableForge/Editor/Data/Tables/"
Move-Item "Assets/TableForge/Editor/DataHandling/TableManager.cs" "Assets/TableForge/Editor/Data/Tables/"

# Move Generation files
Move-Item "Assets/TableForge/Editor/DataHandling/TableGeneration/ItemSelection/*" "Assets/TableForge/Editor/Data/Generation/ItemSelection/"
Move-Item "Assets/TableForge/Editor/DataHandling/TableGeneration/SerializedObjects/*" "Assets/TableForge/Editor/Data/Generation/SerializedObjects/"
Move-Item "Assets/TableForge/Editor/DataHandling/TableGeneration/SerializedType/*" "Assets/TableForge/Editor/Data/Generation/SerializedType/"
```

## Phase 5: Move Serialization Files

```powershell
# Move Serialization files
Move-Item "Assets/TableForge/Editor/DataHandling/Serialization/TableSerializer.cs" "Assets/TableForge/Editor/Serialization/Core/"
Move-Item "Assets/TableForge/Editor/DataHandling/Serialization/SerializationConstants.cs" "Assets/TableForge/Editor/Serialization/Core/"
Move-Item "Assets/TableForge/Editor/DataHandling/Serialization/SerializationArgs/*" "Assets/TableForge/Editor/Serialization/Arguments/"
Move-Item "Assets/TableForge/Editor/DataHandling/Serialization/SerializableData/*" "Assets/TableForge/Editor/Serialization/Data/"
Move-Item "Assets/TableForge/Editor/DataHandling/Serialization/Serializers/*" "Assets/TableForge/Editor/Serialization/Serializers/"
```

## Phase 6: Move UI Files

```powershell
# Move UI Core files
Move-Item "Assets/TableForge/Editor/UI/Attributes/CellControlUsageAttribute.cs" "Assets/TableForge/Editor/UI/Core/Attributes/"
Move-Item "Assets/TableForge/Editor/UI/Enums/*" "Assets/TableForge/Editor/UI/Core/Enums/"
Move-Item "Assets/TableForge/Editor/UI/UiConstants.cs" "Assets/TableForge/Editor/UI/Core/Constants/"

# Move UI Windows
Move-Item "Assets/TableForge/Editor/UI/Windows/AddTabWindow/*" "Assets/TableForge/Editor/UI/Windows/AddTabWindow/"
Move-Item "Assets/TableForge/Editor/UI/Windows/ExportWindow/*" "Assets/TableForge/Editor/UI/Windows/ExportWindow/"
Move-Item "Assets/TableForge/Editor/UI/Windows/ImportWindow/*" "Assets/TableForge/Editor/UI/Windows/ImportWindow/"

# Move UI Metadata
Move-Item "Assets/TableForge/Editor/UI/Metadata/TableMetadata.cs" "Assets/TableForge/Editor/UI/Metadata/"
Move-Item "Assets/TableForge/Editor/UI/Metadata/Data/*" "Assets/TableForge/Editor/UI/Metadata/Data/"

# Move UI Cache
Move-Item "Assets/TableForge/Editor/UI/Cache/SessionCache.cs" "Assets/TableForge/Editor/UI/Cache/"
Move-Item "Assets/TableForge/Editor/UI/Cache/Data/*" "Assets/TableForge/Editor/UI/Cache/Data/"

# Move UI Utilities
Move-Item "Assets/TableForge/Editor/UI/Utils/AssetUtils.cs" "Assets/TableForge/Editor/UI/Utilities/"
Move-Item "Assets/TableForge/Editor/UI/Utils/CellLocator.cs" "Assets/TableForge/Editor/UI/Utilities/"
Move-Item "Assets/TableForge/Editor/UI/Utils/Extensions/*" "Assets/TableForge/Editor/UI/Utilities/Extensions/"
Move-Item "Assets/TableForge/Editor/UI/Utils/SizeCalculation/*" "Assets/TableForge/Editor/UI/Utilities/SizeCalculation/"
Move-Item "Assets/TableForge/Editor/UI/Utils/Undo-Redo/*" "Assets/TableForge/Editor/UI/Utilities/UndoRedo/"

# Move UI Styling
Move-Item "Assets/TableForge/Editor/UI/UssMappings/*" "Assets/TableForge/Editor/UI/Styling/UssMappings/"
Move-Item "Assets/TableForge/Editor/UI/Assets/*" "Assets/TableForge/Editor/UI/Styling/Assets/"

# Move UI Data
Move-Item "Assets/TableForge/Editor/UI/Utils/Data/*" "Assets/TableForge/Editor/UI/Data/"
```

## Phase 7: Update Assembly Definitions

```powershell
# Update Runtime assembly definition
# Edit Assets/TableForge/Runtime/TableForge.Runtime.asmdef to include new folder structure

# Update Editor assembly definition  
# Edit Assets/TableForge/Editor/TableForge.Editor.asmdef to include new folder structure
```

## Phase 8: Update Namespaces

The following files will need namespace updates:

### Runtime Files:
- `Assets/TableForge/Runtime/Core/Attributes/TableForgeIgnoreAttribute.cs`
- `Assets/TableForge/Runtime/Core/DataStructures/SerializedDictionary.cs`

### Editor Files:
- All files in `Assets/TableForge/Editor/Core/`
- All files in `Assets/TableForge/Editor/Data/`
- All files in `Assets/TableForge/Editor/Serialization/`
- All files in `Assets/TableForge/Editor/UI/`

## Phase 9: Update Import Statements

Search and replace import statements throughout the codebase:

```powershell
# Example search and replace patterns:
# Old: using TableForge.Editor.DataHandling.Cells;
# New: using TableForge.Editor.Data.Cells;

# Old: using TableForge.Editor.DataHandling.Serialization;
# New: using TableForge.Editor.Serialization;

# Old: using TableForge.Editor.UI.Utils;
# New: using TableForge.Editor.UI.Utilities;
```

## Phase 10: Clean Up Old Folders

```powershell
# Remove old folders after confirming all files are moved
Remove-Item "Assets/TableForge/Editor/DataHandling" -Recurse -Force
Remove-Item "Assets/TableForge/Runtime/Attributes" -Recurse -Force
Remove-Item "Assets/TableForge/Runtime/DataStructures" -Recurse -Force
```

## Important Notes

1. **Backup First**: Always create a backup before running this script
2. **Unity Refresh**: Unity will need to refresh after folder changes
3. **Meta Files**: Unity will automatically handle .meta file updates
4. **Testing**: Test thoroughly after each phase
5. **Version Control**: Commit changes after each successful phase

## Verification Checklist

- [ ] All files moved to correct locations
- [ ] Namespaces updated correctly
- [ ] Assembly definitions updated
- [ ] Import statements updated
- [ ] No compilation errors
- [ ] All tests pass
- [ ] UI functionality works correctly
- [ ] Serialization works correctly 