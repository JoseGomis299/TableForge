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
  - [Data Types Support](#data-types-support)
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
- **Undo/Redo Support**: Full undo/redo functionality for all operations
- **Type Safety**: Automatic type detection and validation for all data types

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

1. **Open Import Table Window**: Go to `Window > TableForge > Import Table` in the Unity menu
2. **Select Data File**: Choose the CSV or JSON file you want to import
3. **Configure Import Settings**: Map columns, set data types, and adjust import options as needed
4. **Import Table**: Click the "Import" button to complete the import process
5. **Access Imported Table**: After import, the new table will appear in the "+" menu in the TableVisualizer window

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

#### Mathematical Functions
- **SUM(range)**: Sum of values in a range
- **AVERAGE(range)**: Average of values in a range
- **COUNT(range)**: Count of non-null values
- **MAX(range)**: Maximum value in a range
- **MIN(range)**: Minimum value in a range

#### Arithmetic Functions
- **MULTIPLY(value1, value2)**: Multiply two numbers
- **DIVIDE(dividend, divisor)**: Divide two numbers

#### Conditional Functions
- **SUMIF(range, criteria, [sum_range])**: Sum values based on condition

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
- **Modulo**: `%`

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
- **Flatten Sub-tables**: Convert nested objects and collections to flat structure (only in CSV files)

## Advanced Features

### Sub-tables

Complex objects and collections are displayed as expandable sub-tables:
- **Expand/Collapse**: Click the arrow icon to expand or collapse sub-tables
- **Direct Editing**: Edit nested data directly within sub-tables
- **Independent Transposition**: Transpose sub-tables independently from the main table
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
- **Drag Reordering**: Reorder columns by dragging headers
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

- **Equality**: `=` or `==` (equal)
- **Inequality**: `!=` or `<>` (not equal)
- **Greater Than**: `>` (greater than)
- **Less Than**: `<` (less than)
- **Greater or Equal**: `>=` (greater than or equal)
- **Less or Equal**: `<=` (less than or equal)
- **Contains**: `~=` or `=~` (contains text)
- **Not Contains**: `!~` or `~!` (does not contain text)

#### List Values

Filter by list values using bracket notation:

- **Exact Match**: `p:MyList=[1,2,3]` - List must exactly match
- **Contains**: `p:MyList~=[1,2]` - List contains specified items
- **Not Contains**: `p:MyList!~=[1,2]` - List does not contain items
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

#### Table Not Loading
- **Check Asset Types**: Ensure ScriptableObjects are properly serialized with `[System.Serializable]` attributes
- **Verify Namespace**: Check that the correct namespace is selected in the table creation window
- **Asset References**: Ensure assets are not missing, corrupted, or have broken references

#### Formula Errors
- **Syntax Check**: Verify formula syntax, parentheses matching, and cell references
- **Type Compatibility**: Ensure data types are compatible with the functions being used
- **Circular References**: Check for circular formula dependencies that reference each other

#### Import/Export Issues
- **Data Format**: Verify CSV/JSON format is correct and matches the expected structure
- **Column Mapping**: Ensure all required ScriptableObject fields are properly mapped
- **File Permissions**: Check file read/write permissions for import/export operations

#### Performance Issues
- **Table Size**: Consider breaking very large tables (>10,000 cells) into smaller, more manageable tables
- **Formula Complexity**: Simplify complex nested formulas with many cell references

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

## Data Types Support

TableForge supports a wide range of data types:

### Primitive Types
- **Integers**: `int`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `uint`
- **Floating Point**: `float`, `double`
- **Text**: `string`, `char`
- **Boolean**: `bool`
- **Enum**: Any enum type

### Unity Types
- **All types serialized by Unity**

### Collections
- **Arrays**: Any type array
- **Lists**: `List<T>`
- **Dictionaries**: `SerializedDictionary<K,V>`

### Complex Types
- **Custom Classes**: Any serializable class
- **ScriptableObjects**: Any ScriptableObject type
- **Unity Objects**: Any UnityEngine.Object

### Special Handling
- **Nested Objects**: Displayed as expandable sub-tables
- **Collections**: Displayed as sub-tables with individual items
- **Null Values**: Handled gracefully with appropriate display

## Extending TableForge

### Creating Custom Cell Types

To create a custom cell type for a specific data type:

1. **Create the Cell class**:
```csharp
[CellType(TypeMatchMode, typeof(MyCustomType))]
internal class MyCustomCellControl : Cell //if represents a single cell
                                    / PrimitiveBasedCell<PrimitiveType> //if represents a primitive (e.g. int, float...)
                                    / SubTableCell //if represents a unique object subtable
                                    / CollectionCell //if represents a collection of values
{
    // Implementation
}
```

2. **Create Cell Control Class**:
```csharp
[CellControlUsage(typeof(MyCustomCellType), CellSizeCalculationMethod)]
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
- **Real-time Updates**: Activating Table polling can introduce performance issues

### Performance Considerations

- **Large Tables**: Very large tables (>10,000 cells) or with a big number of subtables may impact performance
- **Complex Formulas**: Nested formulas with many references may be slow
- **Memory Usage**: Large datasets consume significant memory

### Type Restrictions

- **Non-serializable Types**: Types not marked as `[System.Serializable]` are not supported
- **Circular References**: Objects with circular references may cause issues
- **Custom Serialization**: Custom serialization logic is not supported

---

**Note**: TableForge is designed for Unity Editor use and requires Unity 2021.3 or later. The tool is optimized for managing ScriptableObject data and may not be suitable for runtime data management.
