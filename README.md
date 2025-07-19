# TableForge - Unity ScriptableObject Management Tool

TableForge is a powerful Unity Editor tool designed for managing, visualizing, and manipulating tabular data from ScriptableObjects. It provides a spreadsheet-like interface with advanced features for data analysis, formula support, and data import/export capabilities.

## Table of Contents

- [Overview](#overview)
- [For Users](#for-users)
  - [Getting Started](#getting-started)
  - [Core Features](#core-features)
  - [Table Visualization](#table-visualization)
  - [Functions and Formulas](#functions-and-formulas)
  - [Import/Export](#importexport)
  - [Advanced Features](#advanced-features)
  - [Row Filtering](#row-filtering)
  - [Troubleshooting](#troubleshooting)
- [For Developers](#for-developers)
  - [Architecture Overview](#architecture-overview)
  - [Core Concepts](#core-concepts)
  - [Field Serialization in TableForge](#field-serialization-in-tableforge)
  - [Extending TableForge](#extending-tableforge)
  - [Contributing](#contributing)
- [Limitations](#limitations)

## Overview

TableForge transforms Unity's ScriptableObject data into interactive, spreadsheet-like tables that can be viewed, edited, and analyzed directly in the Unity Editor. It supports complex data structures, nested objects, collections, and provides Excel-like formula functionality.

### Key Benefits

- **Visual Data Management**: View and edit ScriptableObject data in a familiar spreadsheet format
- **Formula Support**: Use Excel-like functions and formulas for data calculations
- **Data Import/Export**: Import data from CSV/JSON files and export tables to various formats
- **Intelligent Copy/Paste**: Copy and paste values between TableForge and other programs like Excel easily
- **Advanced Filtering**: Filter and search through table data efficiently

---

# For Users

## Getting Started

### Installation

1. Import the TableForge package into your Unity project
2. The tool will be available under `Window > TableForge` in the Unity menu

### Basic Usage

#### Create Your First Table

1. **Open TableVisualizer**: Go to `Window > TableForge > TableVisualizer`
2. **Add a Table**: Click the "+" button in the toolbar to add a new table
3. **Select Data Type**: Choose the ScriptableObject type you want to visualize
4. **Select Assets**: Choose specific assets or bind to all assets of that type
5. **Create Assets (optional)**: Create new ScriptableObject instances directly from the table creation window
6. **Rename Assets (optional)**: Rename existing assets during table creation
7. **View Data**: Your data will be displayed in a spreadsheet format

#### Import and Visualize a Table

1. **Follow The Importing Guide Steps**: [here](#importing-data)
2. **Access Imported Table**: After importing, the new table will appear in the "+" menu in the TableVisualizer window

## Core Features

### Table Management
- **Table Visualization**: Display ScriptableObject data in spreadsheet format
- **Cell Editing**: Direct editing of cell values with type validation
- **Row/Column Management**: Add, remove, reorder rows and columns
- **Data Filtering**: Advanced filtering and search capabilities
- **Table Transposition**: Swap rows and columns for different data views
- **Column Visibility**: Show/hide specific columns as needed
- **Size Management**: Automatic and manual column/row sizing

### Formula System
- **Excel-like Functions**: SUM, AVERAGE, COUNT, MAX, MIN, and more
- **Cell References**: Reference other cells using A1 notation (A1, B2, etc.)
- **Range References**: Reference cell ranges (A1:B5, C1:C10)
- **Arithmetic Operations**: Basic math operations (+, -, *, /, ^, %)
- **Logical Functions**: IF conditions and boolean operations
- **Nested Functions**: Combine multiple functions in complex expressions

### Data Import/Export
- **CSV Import**: Import data from CSV files with column mapping
- **JSON Import**: Import data from JSON files
- **CSV Export**: Export tables to CSV format with various options
- **JSON Export**: Export tables to JSON format
- **Asset Creation**: Automatically create ScriptableObject assets during import

### Advanced Features
- **Sub-tables**: Handle nested objects and collections as expandable sub-tables
- **Type Binding**: Bind tables to specific ScriptableObject types for automatic updates
- **Metadata Persistence**: Save table configurations, layouts, and formulas
- **Session Management**: Remember open tables and their states across Unity sessions
- **Undo/Redo**: Complete undo/redo system for all operations including cell edits, structural changes, and formula modifications
- **Intelligent Copy/Paste**: Seamless data transfer between TableForge and external applications like Excel

## Table Visualization

### Main Interface

The TableVisualizer window provides a complete spreadsheet interface:

- **Toolbar**: Contains table management tools and controls
- **Table Area**: Main spreadsheet display
- **Headers**: Column and row headers with sorting and context menus
- **Cells**: Individual data cells with editing capabilities

### Navigation and Shortcuts

#### Mouse Navigation
- **Click**: Select a single cell
- **Drag**: Select a range of cells
- **Click Header**: Select entire column or row
- **Ctrl+Click**: Add to selection (multi-select)
- **Shift+Click**: Extend selection range
- **Double-click**: Edit cell value
- **Right-click**: Context menu (only on headers)

#### Keyboard Navigation
- **Arrow Keys**: Navigate between cells
- **Enter**: Edit selected cell or enter into sub-table
- **Escape**: Exit from sub-table
- **Shift+Esc**: Exit from sub-table and close its foldout
- **Tab**: Move to next cell
- **Shift+Tab**: Move to previous cell

#### Editing Shortcuts
- **Escape**: Cancel editing
- **Ctrl+Z**: Undo
- **Ctrl+Y**: Redo
- **Ctrl+C**: Copy selected cells
- **Ctrl+V**: Paste into selected cells

#### Formula Shortcuts
- **=**: Start formula entry
- **Ctrl+Shift+C**: Copy selected cells formulas
- **Ctrl+Shift+V**: Paste into selected cells formulas

### Selection Modes

- **Single Cell**: Click on any cell to select it
- **Range Selection**: Drag to select multiple cells
- **Column/Row Selection**: Click on headers to select entire columns/rows
- **Multi-Selection**: Ctrl+Click to select multiple non-contiguous cells
- **Extended Selection**: Shift+Click to extend selection range

### Editing Features

- **Direct Editing**: Double-click or press Enter to edit cell values
- **Type Validation**: Automatic validation of data types
- **Formula Entry**: Start with "=" to enter formulas
- **Copy/Paste**: Intelligent copy/paste with format preservation
- **Auto-complete**: Formula suggestions and cell reference completion

## Functions and Formulas

### Formula Syntax

Formulas start with "=" and can contain:
- **Functions**: `=SUM(A1:A5)`
- **Cell References**: `=A1 + B2`
- **Constants**: `=10 * 2`
- **Arithmetic**: `=(A1 + B2) * C3`

### Available Functions

TableForge supports a comprehensive set of Excel-like functions. Arguments in square brackets `[]` are optional.

#### Mathematical Functions

**SUM(arg1, [arg2, ...])**
- **Description**: Sums a series of values or ranges. Only compatible with numeric values.
- **Example**: `=SUM(A1:A10, B1)` → Sums cells A1 to A10 plus B1.

**AVERAGE(arg1, [arg2, ...])**
- **Description**: Calculates the average of a series of values or ranges. Only compatible with numeric values.
- **Example**: `=AVERAGE(A1:A10)` → Averages the values in A1:A10.

**COUNT(arg1, [arg2, ...])**
- **Description**: Counts the number of cells with non-null values in a range.
- **Example**: `=COUNT(A1:A10)` → Counts how many cells in A1:A10 have values.

**MAX(arg1, [arg2, ...])**
- **Description**: Returns the maximum value from a set of values or ranges.
- **Example**: `=MAX(A1:A10, 100)` → Returns the highest value between A1:A10 and 100.

**MIN(arg1, [arg2, ...])**
- **Description**: Returns the minimum value from a set of values or ranges.
- **Example**: `=MIN(A1:A10, 0)` → Returns the lowest value between A1:A10 and 0.

**ABS(value)**
- **Description**: Returns the absolute value of the given number (without sign).
- **Example**: `=ABS(-5)` → Returns 5.

**ROUND(value, [decimals])**
- **Description**: Rounds a number to the specified number of decimal places. If decimals are not specified, assumes 0 decimal places.
- **Example**: `=ROUND(3.14159, 2)` → Returns 3.14.

#### Arithmetic Functions

**DIVIDE(dividend, divisor)**
- **Description**: Returns the result of dividing one number by another.
- **Example**: `=DIVIDE(10, 2)` → Returns 5.

**MULTIPLY(value1, value2)**
- **Description**: Returns the result of multiplying two values.
- **Example**: `=MULTIPLY(3, 4)` → Returns 12.

**MOD(dividend, divisor)**
- **Description**: Returns the remainder of dividing one number by another.
- **Example**: `=MOD(10, 3)` → Returns 1.

#### Conditional Functions

**IF(condition, value_if_true, [value_if_false])**
- **Description**: Evaluates a condition and returns a value based on whether it's true or false. The result value must be compatible with the cell type.
- **Example**: `=IF(A1 > 10, "Yes", "No")` → Returns "Yes" if A1 is greater than 10.

**SUMIF(range, criteria, [sum_range])**
- **Description**: Sums cells in a range that meet a specific criteria. Only compatible with numeric values.
- **Example**: `=SUMIF(A1:A10, ">5", B1:B10)` → Sums values in B1:B10 where A1:A10 is greater than 5.

**COUNTIF(range, criteria)**
- **Description**: Counts cells that meet a specific criteria.
- **Example**: `=COUNTIF(A1:A10, "Red")` → Counts cells containing the text "Red".

#### Logical Functions

**AND(condition1, [condition2, ...])**
- **Description**: Returns TRUE if all conditions are true.
- **Example**: `=AND(A1 > 0, B1 < 10)` → Returns TRUE if both conditions are met.

**OR(condition1, [condition2, ...])**
- **Description**: Returns TRUE if any of the conditions is true.
- **Example**: `=OR(A1 > 0, B1 < 10)` → Returns TRUE if at least one condition is met.

**NOT(condition)**
- **Description**: Inverts the logical value of the condition.
- **Example**: `=NOT(A1 = 5)` → Returns TRUE if A1 is not equal to 5.

**XOR(condition1, [condition2, ...])**
- **Description**: Returns TRUE if one and only one of the conditions is true.
- **Example**: `=XOR(A1=1, B1=1)` → Returns TRUE if only one of the two conditions is met.

### Cell References

#### Single Cell References
- **A1**: Column A, Row 1
- **$A$1**: Absolute reference (doesn't change when copied)
- **A$1**: Mixed reference (absolute row, relative column)
- **$A1**: Mixed reference (absolute column, relative row)

#### Range References
- **A1:B5**: Range from A1 to B5
- **A:A**: Entire column A
- **1:5**: Rows 1 through 5

#### Cross-Table References
- **A1.B2**: Reference cell B2 in sub-table A1
- **A1.B2:A1.C5**: Reference range B2:C5 in sub-table A1

### Arithmetic Operations

Supported operators:
- **Addition**: `+`
- **Subtraction**: `-`
- **Multiplication**: `*`
- **Division**: `/`
- **Exponentiation**: `^`
- **Percentage**: `%` (Divides a number by 100 when written at the end of it)

### Logical Operations

Supported comparisons:
- **Equal**: `=`
- **Not Equal**: `<>` or `!=`
- **Less Than**: `<`
- **Greater Than**: `>`
- **Less Than or Equal**: `<=`
- **Greater Than or Equal**: `>=`

## Import/Export

### Importing Data

#### CSV Import
1. Go to `Window > TableForge > Import Table`
2. Select CSV format
3. Choose your data type and namespace
4. Paste CSV data or import from file using the "Import from File" button
5. Map columns to ScriptableObject fields
6. Review the generated assets and finalize import

#### JSON Import
1. Select JSON format in the import window
2. Provide JSON data in the text field or import it from a file using the "Import from File" button
3. Map fields to ScriptableObject properties
4. Review and finalize import

**Expected JSON Format**

The expected JSON format for importing is the same as when exporting to JSON. The structure is as follows:

```json
{
  "items": [
    {
      "guid": "optional-guid-string",         // Optional: Only if GUIDs are included
      "path": "optional/asset/path.asset",    // Optional: Only if paths are included
      "properties": {
        "FieldA": "ValueA",
        "FieldB": 123,
        "FieldC": true,
        "NestedObject": {
          "SubField1": "SubValue1"
        },
        "ListField": [1, 2, 3],
        "DictionaryField": {
          "key1": "value1"
          "key2": "value2"
        }
      }
    }
    , ...
  ]
}
```

- Each item represents a row and contains a `properties` object with key-value pairs for each field.
- `guid` and `path` are optional.
- Nested objects, dictionaries and lists are supported in the `properties` section.

You can use exported JSON files as templates for import.

#### Import Options
- **Table Name**: Name for the new table
- **Base Path**: Folder where new ScriptableObject assets will be created
- **Base Name**: Naming pattern for newly created assets
- **Header Row**: Whether the first row contains column headers (only when importing from CSV)
- **Column Mapping**: Map CSV/JSON columns to ScriptableObject fields

### Exporting Data

#### CSV Export
1. Go to `Window > TableForge > Export Table`
2. Select your table metadata from the dropdown
3. Choose CSV format
4. Configure export options:
   - **Include GUIDs**: Export Unity asset GUIDs for reference
   - **Include Paths**: Export Unity asset file paths
   - **Flatten Sub-tables**: Convert nested data to flat structure
5. Preview the export format
6. Export to file using the "Export Table" button

#### JSON Export
1. Select JSON format from the dropdown
2. Configure options (Include GUIDs, Include Paths)
3. Preview the JSON structure
4. Export to file

#### Export Options
- **Include GUIDs**: Export Unity asset GUIDs for asset identification
- **Include Paths**: Export Unity asset file paths for external reference
- **Flatten Sub-tables**: Convert nested objects to flat structure (only in CSV files) (take into account that when importing a CSV file it is expected to NOT be flattened)

## Advanced Features

### Sub-tables

Complex objects and collections are displayed as expandable sub-tables:
- **Expand/Collapse**: Click the arrow icon to expand or collapse sub-tables
- **Direct Editing**: Edit nested data directly within sub-tables
- **Cross-table Formulas**: Reference cells across different sub-tables using dot notation (e.g., A1.B2)

### Type Binding

Bind tables to specific ScriptableObject types for dynamic updates:
- **Automatic Updates**: Tables automatically update when new assets of the bound type are added to the project
- **Type Safety**: Ensures data consistency and prevents type mismatches
- **Performance Optimization**: Efficiently handles large collections of assets

### Table Transposition

Swap rows and columns for different data perspectives:
- **Main Tables**: Transpose entire tables with a single click
- **State Persistence**: Transposition state is automatically saved and restored

### Column Management

Comprehensive column control features:
- **Visibility Toggle**: Show or hide specific columns using the column visibility dropdown
- **Size Control**: Manual resizing or automatic sizing based on content
- **Sorting**: Sort data by clicking column headers (ascending/descending)

### Row Filtering

Advanced row filtering system with powerful search capabilities:
- **Search Bar**: Located in the toolbar, allows filtering rows based on various criteria
- **Manual Activation**: Filter is applied when you press Enter or when the search field loses focus
- **Complex Expressions**: Support for boolean logic, parentheses grouping, and multiple conditions
- **Multiple Data Sources**: Filter by GUID, path, name, or cell values

#### Filter Identifiers

Use the following identifiers to specify what to search for:

- **`g` or `guid`**: Search by Unity asset GUID
  - Example: `g:1234567890abcdef` - Find row with specific GUID
- **`path`**: Search by asset file path (relative to Assets folder)
  - Example: `path:ScriptableObjects/MyAsset.asset` - Find specific asset
  - Example: `path:ScriptableObjects` - Find all assets in folder
- **`n` or `name`**: Search by row name
  - Example: `n:Player` - Find rows containing "Player" in name
- **`p` or `property`**: Search by cell values
  - Example: `p:Health>100` - Find rows where Health column > 100
  - Example: `p:$A=50` - Find rows where first column = 50

#### Column References

When filtering by properties, you can reference columns in two ways:

- **Column Name**: Use the exact column name
  - Example: `p:PlayerName~=John` - Search in "PlayerName" column
- **Column Letter**: Use `$` followed by the column letter
  - Example: `p:$B>10` - Search in second column (B)
  - Example: `p:$A~=Test` - Search in first column (A)

#### Nested Property Access

Access properties in sub-tables using dot notation:

- **Single Level**: `p:SubTable.FieldName=value`
  - Example: `p:Inventory.WeaponCount>5`
- **Multiple Levels**: `p:SubTable.SubSubTable.Field=value`
  - Example: `p:Player.Inventory.WeaponCount>3`
- **Mixed Notation**: Combine column letters and names
  - Example: `p:$B.Field>10` - Second column, field with name Field > 10

#### Comparison Operators

Available comparison operators for property filtering:

- **Equality**: `=` or `==` 
- **Inequality**: `!=` or `<>` 
- **Greater Than**: `>` 
- **Less Than**: `<` 
- **Greater or Equal**: `>=` 
- **Less or Equal**: `<=` 
- **Contains**: `~=` or `=~` 
- **Not Contains**: `!~` or `~!` 

#### List Values

Filter by list values using bracket notation:

- **Exact Match**: `p:MyList=[1,2,3]` - List must exactly match
- **Contains**: `p:MyList~=[1,2]` - List contains specified items
- **Not Contains**: `p:MyList!~[1,2]` - List does not contain items
- **Length Comparison**: `p:MyList>2` - List has more than 2 items

#### Boolean Logic

Combine multiple conditions using logical operators:

- **AND**: `&` or `&&` (both conditions must be true)
- **OR**: `|` or `||` (either condition can be true)
- **NOT**: `!` (negates the condition)
- **Parentheses**: `()` for grouping and precedence

#### Filter Examples

**Simple Filters:**
- `n:Player` - Rows with "Player" in the name
- `p:Health>100` - Rows where Health > 100
- `g:1234567890abcdef` - Row with specific GUID

**Complex Filters:**
- `n:Player & p:Health>100` - Players with health > 100
- `(p:Level>10 | p:Experience>1000) & p:Class=Warrior` - High-level or experienced warriors
- `p:Inventory.WeaponCount>5 & !(g:1234567890abcdef)` - Players with many weapons, excluding specific GUID

**Advanced Filters:**
- `p:$A~=Test & p:$B>10 & p:$C.$A=50` - Complex nested filtering
- `p:Tags~=[Fire, Ice] & p:Level>=20` - Characters with specific tags and minimum level
- `(n:Enemy | n:Boss) & p:Health>500` - Enemies or bosses with high health
- `p:[10, 20, 30] ~= Attack`          - Attack is 10, 20 or 30

### Undo/Redo System

Comprehensive undo/redo functionality:
- **Cell Operations**: Undo individual cell edits and value changes
- **Structural Changes**: Undo row/column additions, deletions, and reordering
- **Formula Operations**: Undo formula modifications and calculations
- **Batch Operations**: Undo multiple related operations as a single action

## Troubleshooting

### Common Issues

#### Formula Errors
- **Syntax Check**: Verify formula syntax, parentheses matching, and cell references
- **Type Compatibility**: Ensure data types are compatible with the functions being used
- **Circular References**: Check for circular formula dependencies that reference each other

#### Import/Export Issues
- **Data Format**: Verify CSV/JSON format is correct and matches the expected structure
- **File Permissions**: Check file read/write permissions for import/export operations

#### Performance Issues
- **Table Size**: Consider breaking very large tables (>10,000 cells) into smaller, more manageable tables
- **Formula Complexity**: Simplify complex nested formulas with many cell references
- **Data polling**: Activating data polling can introduce performance issues

### Getting Help

For additional support:
1. Check the Unity Console for detailed error messages and warnings
2. Review the TableForge logs in the Console window for debugging information
3. Verify your data types and ScriptableObject structure match the supported types
4. Test with simple data first before using complex nested structures
5. Ensure all ScriptableObject classes are properly serialized with `[System.Serializable]` attributes or are public

---

# For Developers

## Architecture Overview

TableForge is built with a modular architecture designed for extensibility and maintainability:

### Core Components

- **TableControl**: Main orchestrator for table visualization and interaction
- **HeaderControl**: Base class for row and column headers with context menu support
- **CellControl**: Abstract base for all cell types with selection and editing capabilities
- **VisibilityManager**: Handles virtual scrolling for performance optimization
- **CellSelector**: Manages cell selection and navigation
- **ContextMenuBuilder**: Modular system for building context menus

## Core Concepts

### Tables

A table represents a collection of ScriptableObject instances displayed in a spreadsheet format. Each row represents one ScriptableObject, and each column represents a field from that object.

### Cells

Cells are the individual data containers within a table. Each cell can contain:
- Simple values (strings, numbers, booleans)
- Complex objects (displayed as sub-tables)
- Collections (arrays, lists)

### Metadata

Table metadata stores configuration information such as:
- Column visibility settings
- Column/row order
- Cell sizes
- Formulas
- Table transposition state

### Cell Anchors

Cell anchors represent the structural elements of a table:
- **Columns**: Vertical data containers
- **Rows**: Horizontal data containers
- Each anchor has a position, name, and unique ID

## Field Serialization in TableForge

TableForge serializes fields using the same rules as Unity:

- **Public fields** are serialized by default.
- **Private fields** marked with `[SerializeField]` are serialized.
- **Properties** with `[field: SerializeField]` are serialized.
- **Unity Types** (e.g., `Vector3`, `Color`, `AnimationCurve`, etc.) and any type Unity can serialize are supported.
- **Custom Classes/Structs** must be marked `[System.Serializable]` to be serialized.
- **Collections** (arrays, lists, serializedDictionaries) are supported if Unity can serialize them.
- **If Unity can serialize it, TableForge can as well.**

### Excluding Fields: `[TableForgeIgnore]` Attribute

You can prevent TableForge from serializing a field (even if Unity would) by adding the `[TableForgeIgnore]` attribute:

```csharp
using TableForge.Attributes;

public class ExampleClass : ScriptableObject
{
    public int IncludedField;
    [TableForgeIgnore]
    public string IgnoredField;
}
```

In this example, `IncludedField` will be serialized and shown in TableForge, but `IgnoredField` will not.

### Type Restrictions

- **Non-serializable Types**: Custom types not marked as `[System.Serializable]` are not supported.
- **Circular References**: Objects with circular references will not be serialized.
- **Unity Serialization Limitations**: If Unity cannot serialize a field, neither can TableForge.

For more details, see [Unity's serialization documentation](https://docs.unity3d.com/Manual/script-Serialization.html).

## Extending TableForge

### Creating Custom Cell Types

To create a custom cell type for a specific data type:

1. **Create the Cell class**:
```csharp
[CellType(TypeMatchMode, typeof(MyCustomType))]
internal class MyCustomCell : Cell //if represents a single cell
                              / PrimitiveBasedCell<PrimitiveType> //if represents a primitive (e.g. int, float...)
                              / SubTableCell //if represents a unique object subtable
                              / CollectionCell //if represents a collection of values
{
    // Implementation
}
```
2. **Create a CellSerializer class (if necessary)**
```csharp
internal class MyCustomCellSerializer : CellSerializer
{
    // Implementation
}

//In MyCustomCell constructor
Serializer = new MyCustomCellSerializer(this);
```

3. **Create Cell Control class**:
```csharp
[CellControlUsage(typeof(MyCustomCellType), CellSizeCalculationMethod)] //always
[SubTableCellControlUsage(TableType, TableReorderMode, RowHeaderVisibility, ColumnHeaderVisibility)] //if represents a subtable
internal class MyCustomCellControl : SimpleCellControl //if represents a single cell which value is not given by text
                                   / TextBasedCellControl<Type> //if represents a single cell which value is given by text
                                   / DynamicSubTableCellControl //if represents a subtable that can have 0 or more rows
{
    //Implementation
}
```

### Creating Custom Context Menus

To add custom context menu functionality:

1. **Extend BaseHeaderContextMenuBuilder**:
```csharp
internal class MyCustomContextMenuBuilder : BaseHeaderContextMenuBuilder
{
    public override void BuildContextMenu(HeaderControl header, ContextualMenuPopulateEvent evt)
    {
        //Implementation
    }
}
```

2. **Register in Header Control**:
```csharp
protected override IHeaderContextMenuBuilder GetContextMenuBuilder()
{
    return new MyCustomContextMenuBuilder();
}
```

### Adding New Functions

To add new formula functions:

1. **Create Function Class**:
```csharp
internal class MyCustomFunction : ExcelFunctionBase
{
    //Implementation
}
```

## Contributing

### Development Setup

1. **Clone the Repository**: Get the latest source code
2. **Open in Unity**: Open the project in Unity 2021.3 or later
3. **Install Dependencies**: Ensure all required packages are installed
4. **Run Tests**: Execute the test suite to verify everything works

#### Recommended .gitignore Settings

It is recommended to add the following directories to your `.gitignore` file to avoid tracking local cache and user-specific settings:

```
*/TableForge/Editor/PersistentData/Cache/
*/TableForge/Editor/PersistentData/Settings/
```

These folders store local cache and editor settings that are specific to your development environment and should not be committed to version control.

### Code Style Guidelines

- **Naming**: Use PascalCase for public members, camelCase for private members
- **Documentation**: Add XML documentation for all public APIs
- **Regions**: Use regions to organize code sections
- **Error Handling**: Use proper exception handling and validation

### Testing

- **Unit Tests**: Write unit tests for new functionality
- **Integration Tests**: Test integration with Unity systems
- **UI Tests**: Test user interface interactions

### Pull Request Process

1. **Create Feature Branch**: Branch from main for new features
2. **Implement Changes**: Follow coding guidelines
3. **Add Tests**: Include appropriate tests
4. **Update Documentation**: Update relevant documentation
5. **Submit PR**: Create pull request with clear description

---

## Limitations

### Current Limitations

- **Dictionary Values**: Adding values to dictionaries is not supported yet

### Type Restrictions

See [Field Serialization in TableForge](#field-serialization-in-tableforge) for details on what types are supported and how serialization works.

---

**Note**: TableForge is designed for Unity Editor use and requires Unity 2022.3 or later. The tool is optimized for managing ScriptableObject data and may not be suitable for runtime data management.
