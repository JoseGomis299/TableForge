# TableForge Demo Data

This folder contains example ScriptableObject assets and data types for use with the TableForge tool. You can use these demo assets to quickly create tables, experiment with TableForge's features, and export data for testing or learning purposes.

---

## How to Create Tables Using Demo Data

1. **Open TableForge TableVisualizer**
   - In Unity, go to `Window > TableForge > TableVisualizer`.

2. **Add a New Table**
   - Click the `+` button in the TableVisualizer toolbar.
   - In the dialog, select the data type you want to visualize (e.g., `CharacterStats`, `EnemyStats`, or `WeaponStats`).
   - Choose to bind the table to all assets of that type, or select specific assets from the Demo/Data folders.

3. **View and Edit Data**
   - The table will display the selected ScriptableObject assets as rows, with their fields as columns.
   - You can edit values directly in the table, use formulas, and explore sub-tables for nested data.

---

## How to Export Demo Data

1. **Open the Export Window**
   - Go to `Window > TableForge > Export Table`.

2. **Select the Table to Export**
   - Choose the table you created from the dropdown list.

3. **Configure Export Options**
   - Select the desired export format (CSV or JSON).
   - Choose whether to include GUIDs, asset paths, or flatten sub-tables (see TableForge documentation for details).

4. **Export the Data**
   - Preview the export if desired.
   - Click the `Export Table` button to save the data to a file.

---

## Tips
- You can import the exported data back into TableForge using the Import Table window.
- Use the Demo/Data/Characters, Demo/Data/Enemies, and Demo/Data/Weapons folders for ready-to-use assets.
- Try editing values and formulas in the table to see real-time updates.

For more details, see the main TableForge [README](../../../../README.md). 