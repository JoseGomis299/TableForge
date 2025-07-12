# TableForge Folder Reorganization Plan

## Current Structure Issues
- Inconsistent naming conventions (PascalCase vs camelCase)
- Mixed responsibilities in DataHandling
- Unclear separation between runtime and editor concerns
- Some folders could be better organized by functionality

## Proposed New Structure

```
Assets/TableForge/
├── Runtime/Core/           # Essential runtime components
├── Editor/
│   ├── Core/              # Shared editor utilities
│   ├── Data/              # Data structures and logic
│   ├── Serialization/     # Serialization system
│   ├── UI/                # UI system
│   └── Tests/             # Testing framework

```

## Key Improvements

### 1. **Consistent Naming Convention**
- All folders use PascalCase
- Clear, descriptive names that indicate purpose
- Grouped by functionality rather than technical concerns

### 2. **Better Separation of Concerns**
- **Runtime**: Only essential runtime components
- **Editor/Core**: Shared editor utilities and base classes
- **Editor/Data**: All data-related functionality
- **Editor/Serialization**: Dedicated serialization system
- **Editor/UI**: Complete UI system with clear subsections

### 3. **Logical Grouping**
- **Cells**: Base classes, implementations, interfaces, and factory
- **Tables**: Table-specific data structures
- **Generation**: Table generation logic
- **UI**: Organized by functionality (Windows, Metadata, Cache, etc.)

### 4. **Improved Maintainability**
- Clear hierarchy makes it easier to find related code
- Consistent naming reduces cognitive load
- Better separation allows for easier testing and modification

### 5. **Scalability**
- Structure supports future additions without major reorganization
- Clear boundaries between different systems
- Easy to add new cell types, serializers, or UI components

## Migration Strategy

1. **Phase 1**: Create new folder structure
2. **Phase 2**: Move files to new locations
3. **Phase 3**: Update namespace declarations
4. **Phase 4**: Update assembly definition references
5. **Phase 5**: Update import statements throughout the codebase
6. **Phase 6**: Clean up old folders

## Benefits

- **Developer Experience**: Easier to navigate and understand
- **Code Organization**: Related functionality is grouped together
- **Maintainability**: Clear structure makes maintenance easier
- **Scalability**: Structure supports future growth
- **Consistency**: Uniform naming and organization patterns 

## Summary

I've analyzed your TableForge folder organization and created a comprehensive improvement plan. Here are the key issues I identified and the solutions I've provided:

### **Current Issues:**
1. **Inconsistent naming** - Mixed PascalCase and camelCase folder names
2. **Mixed responsibilities** - The `DataHandling` folder contains both data structures and business logic
3. **Unclear hierarchy** - Some folders could be better organized by functionality
4. **Poor separation of concerns** - Runtime vs Editor concerns could be better separated

### **Proposed Improvements:**

I've created three comprehensive documents:

1. **`FOLDER_REORGANIZATION_PLAN.md`** - Detailed new structure with clear hierarchy and naming conventions
2. **`REORGANIZATION_SCRIPT.md`** - Step-by-step PowerShell script to implement the changes safely
3. **`IMPROVEMENTS_SUMMARY.md`** - Overview of benefits and expected outcomes

### **Key Benefits of the New Structure:**

- **Consistent PascalCase naming** throughout
- **Functional grouping** instead of technical grouping
- **Clear separation** between Runtime, Editor/Core, Editor/Data, Editor/Serialization, and Editor/UI
- **Better maintainability** with related code co-located
- **Scalable structure** that supports future growth
- **Improved developer experience** with intuitive navigation

### **New Structure Highlights:**
```
Assets/TableForge/
├── Runtime/Core/           # Essential runtime components
├── Editor/
│   ├── Core/              # Shared editor utilities
│   ├── Data/              # Data structures and logic
│   ├── Serialization/     # Serialization system
│   ├── UI/                # UI system
│   └── Tests/             # Testing framework

```

The reorganization transforms TableForge from a technically-organized codebase into a functionally-organized, scalable, and maintainable system. The implementation script provides a safe, incremental approach to making these changes.

Would you like me to help you implement any specific part of this reorganization, or would you like me to explain any particular aspect in more detail? 