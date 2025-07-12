# TableForge - Unity Table Management Tool

TableForge is a powerful Unity Editor tool designed for managing, visualizing, and manipulating tabular data from ScriptableObjects. It provides a spreadsheet-like interface with advanced features for data analysis, formula support, and data import/export capabilities.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Getting Started](#getting-started)
- [Core Concepts](#core-concepts)
- [Data Types Support](#data-types-support)
- [Table Visualization](#table-visualization)
- [Functions and Formulas](#functions-and-formulas)
- [Import/Export](#importexport)
- [Advanced Features](#advanced-features)
- [Limitations](#limitations)
- [Troubleshooting](#troubleshooting)

## Overview

TableForge transforms Unity's ScriptableObject data into interactive, spreadsheet-like tables that can be viewed, edited, and analyzed directly in the Unity Editor. It supports complex data structures, nested objects, collections, and provides Excel-like formula functionality.

### Key Benefits

- **Visual Data Management**: View and edit ScriptableObject data in a familiar spreadsheet format
- **Formula Support**: Use Excel-like functions and formulas for data calculations
- **Data Import/Export**: Import data from CSV/JSON files and export tables to various formats
- **Inteligent Copy/Paste**: Copy and paste values between TableForge and other programs like excel easily
- **Advanced Filtering**: Filter and search through table data efficiently
- **Undo/Redo Support**: Full undo/redo functionality for all operations
- **Type Safety**: Automatic type detection and validation for all data types

## Features

### Core Features

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
- **Type Binding**: Bind tables to specific ScriptableObject types
- **Metadata Persistence**: Save table configurations and layouts
- **Session Management**: Remember open tables across Unity sessions
- **Undo/Redo**: Complete undo/redo system for all operations
- **Copy/Paste**: Copy and paste data between cells and external applications

## Getting Started

### Installation

1. Import the TableForge package into your Unity project
2. The tool will be available under `Window > TableForge` in the Unity menu

### Basic Usage

1. **Open TableVisualizer**: Go to `Window > TableForge > TableVisualizer`
2. **Add a Table**: Click the "+" button in the toolbar to add a new table
3. **Select Data Type**: Choose the ScriptableObject type you want to visualize
4. **Select Assets**: Choose specific assets or bind to all assets of that type
5. **View Data**: Your data will be displayed in a spreadsheet format

### Creating Your First Table

1. Create a ScriptableObject class with your data fields
2. Create instances of your ScriptableObject in the project
3. Open TableVisualizer and add a new table
4. Select your ScriptableObject type and assets
5. Start viewing and editing your data

## Core Concepts

### Tables

A table represents a collection of ScriptableObject instances displayed in a spreadsheet format. Each row represents one ScriptableObject, and each column represents a field from that object.

### Cells

Cells are the individual data containers within a table. Each cell can contain:
- Simple values (strings, numbers, booleans)
- Complex objects (displayed as sub-tables)
- Collections (arrays, lists)
- Formulas (calculated values)

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
- **Integers**: `int`, `long`, `short`, `byte`, `uint`
- **Floating Point**: `float`, `double`
- **Text**: `string`, `char`
- **Boolean**: `bool`
- **Enum**: Any enum type

### Unity Types
- **Vectors**: `Vector2`, `Vector3`, `Vector4`
- **Colors**: `Color`
- **Rects**: `Rect`, `RectOffset`
- **Bounds**: `Bounds`
- **Quaternion**: `Quaternion`
- **AnimationCurve**: `AnimationCurve`
- **Gradient**: `Gradient`
- **LayerMask**: `LayerMask`

### Collections
- **Arrays**: Any type array
- **Lists**: `List<T>`, `IList<T>`
- **Dictionaries**: `Dictionary<K,V>`, `SerializedDictionary<K,V>`

### Complex Types
- **Custom Classes**: Any serializable class
- **ScriptableObjects**: Any ScriptableObject type
- **Unity Objects**: Any UnityEngine.Object

### Special Handling
- **Nested Objects**: Displayed as expandable sub-tables
- **Collections**: Displayed as sub-tables with individual items
- **Null Values**: Handled gracefully with appropriate display

## Table Visualization

### Main Interface

The TableVisualizer window provides a complete spreadsheet interface:

- **Toolbar**: Contains table management tools and controls
- **Table Area**: Main spreadsheet display
- **Headers**: Column and row headers with sorting and context menus
- **Cells**: Individual data cells with editing capabilities

### Navigation

- **Mouse**: Click to select cells, drag to select ranges
- **Keyboard**: Arrow keys to navigate, Enter to edit
- **Scroll**: Mouse wheel or scrollbars to navigate large tables

### Selection

- **Single Cell**: Click on any cell to select it
- **Range Selection**: Drag to select multiple cells
- **Column/Row Selection**: Click on headers to select entire columns/rows
- **Multi-Selection**: Ctrl+Click to select multiple non-contiguous cells

### Editing

- **Direct Editing**: Double-click or press Enter to edit cell values
- **Type Validation**: Automatic validation of data types
- **Formula Entry**: Start with "=" to enter formulas
- **Copy/Paste**: Standard copy/paste operations supported

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
- **A1.B2:C5**: Reference range B2:C5 in sub-table A1

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
4. Paste CSV data or import from file
5. Map columns to fields
6. Review and finalize import

#### JSON Import
1. Select JSON format in the import window
2. Provide JSON data
3. Map fields and finalize import

#### Import Options
- **Table Name**: Name for the new table
- **Base Path**: Folder for new assets
- **Base Name**: Naming pattern for new assets
- **Header Row**: Whether first row contains headers
- **Column Mapping**: Map CSV columns to ScriptableObject fields

### Exporting Data

#### CSV Export
1. Go to `Window > TableForge > Export Table`
2. Select your table metadata
3. Choose CSV format
4. Configure export options:
   - Include GUIDs
   - Include Paths
   - Flatten Sub-tables
5. Export to file

#### JSON Export
1. Select JSON format
2. Configure options (GUIDs, Paths)
3. Export to file

#### Export Options
- **Include GUIDs**: Export Unity asset GUIDs
- **Include Paths**: Export Unity asset paths
- **Flatten Sub-tables**: Convert nested data to flat structure
- **Preview**: See export format before saving

## Advanced Features

### Sub-tables

Complex objects and collections are displayed as sub-tables:
- **Expandable**: Click to expand/collapse sub-tables
- **Editable**: Edit nested data directly
- **Transposable**: Transpose sub-tables independently
- **Formula Support**: Use formulas across sub-tables

### Type Binding

Bind tables to specific ScriptableObject types:
- **Automatic Updates**: Tables update when new assets are added
- **Type Safety**: Ensures data consistency
- **Performance**: Optimized for large asset collections

### Table Transposition

Swap rows and columns:
- **Main Tables**: Transpose entire tables
- **Sub-tables**: Transpose individual sub-tables
- **Persistent**: Transposition state is saved

### Column Management

Advanced column features:
- **Visibility**: Show/hide columns
- **Reordering**: Drag to reorder columns
- **Resizing**: Manual and automatic sizing
- **Sorting**: Sort by column values

### Filtering and Search

Powerful filtering capabilities:
- **Text Search**: Search across all cells
- **Column Filters**: Filter by specific columns
- **Real-time**: Instant search results
- **Case Insensitive**: Flexible matching

### Undo/Redo System

Complete undo/redo support:
- **Cell Changes**: Undo individual cell edits
- **Structural Changes**: Undo row/column operations
- **Formula Changes**: Undo formula modifications
- **Batch Operations**: Undo multiple operations at once

## Limitations

### Current Limitations

- **Graphs**: Graph visualization is not yet implemented
- **Read-only Fields**: Read-only field support is limited
- **Dictionary Values**: Adding values to dictionaries is not fully supported
- **Real-time Updates**: Some external changes may require manual refresh

### Performance Considerations

- **Large Tables**: Very large tables (>10,000 cells) may impact performance
- **Complex Formulas**: Nested formulas with many references may be slow
- **Memory Usage**: Large datasets consume significant memory

### Type Restrictions

- **Non-serializable Types**: Types not marked as `[System.Serializable]` are not supported
- **Circular References**: Objects with circular references may cause issues
- **Custom Serialization**: Custom serialization logic is not supported

## Troubleshooting

### Common Issues

#### Table Not Loading
- **Check Asset Types**: Ensure ScriptableObjects are properly serialized
- **Verify Namespace**: Check that the correct namespace is selected
- **Asset References**: Ensure assets are not missing or corrupted

#### Formula Errors
- **Syntax Check**: Verify formula syntax and cell references
- **Type Compatibility**: Ensure data types are compatible with functions
- **Circular References**: Check for circular formula dependencies

#### Import/Export Issues
- **Data Format**: Verify CSV/JSON format is correct
- **Column Mapping**: Ensure all required fields are mapped
- **File Permissions**: Check file read/write permissions

#### Performance Issues
- **Table Size**: Consider breaking large tables into smaller ones
- **Formula Complexity**: Simplify complex formulas
- **Memory Usage**: Close unused tables to free memory

### Error Messages

#### "No cell control found for cell type"
- **Solution**: Ensure the data type is supported by TableForge
- **Workaround**: Use a supported type or create a custom cell type

#### "Invalid function arguments"
- **Solution**: Check function syntax and argument types
- **Workaround**: Use simpler formulas or different functions

#### "Table name already exists"
- **Solution**: Choose a different table name
- **Workaround**: Delete existing table with same name

### Getting Help

For additional support:
1. Check the Unity Console for detailed error messages
2. Review the TableForge logs in the Console window
3. Verify your data types and ScriptableObject structure
4. Test with simple data first before using complex structures

---

**Note**: TableForge is designed for Unity Editor use and requires Unity 2021.3 or later. The tool is optimized for managing ScriptableObject data and may not be suitable for runtime data management.
