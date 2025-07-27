using System.Collections.Generic;
using NUnit.Framework;
using TableForge.Editor;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using TableForge.Editor.UI;
using UnityEngine.UIElements;

namespace TableForge.Tests
{
    internal class TablePerformanceTests
    {
        private const int REPEAT_COUNT = 100;

        private List<string> GetOrCreateTestScriptableObjects(int rowCount)
        {
            var guids = new List<string>();
            for (int i = 0; i < rowCount; i++)
            {
                string path = $"{PathUtil.GetAndCreateTestDataFolder()}/PerfTestData_{i}.asset";
                var data = AssetDatabase.LoadAssetAtPath<SampleTestData>(path);
                if (data == null)
                {
                    data = ScriptableObject.CreateInstance<SampleTestData>();
                    AssetDatabase.CreateAsset(data, path);
                }
                guids.Add(AssetDatabase.AssetPathToGUID(path));
            }
            return guids;
        }

        private void GenerateTable(int rowCount)
        {
            var metadata = CreateTable(rowCount);

            var times = new List<long>();
            for (int i = 0; i < REPEAT_COUNT; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                TableMetadataManager.GetTable(metadata);
                stopwatch.Stop();
                times.Add(stopwatch.ElapsedMilliseconds);
            }

            long min = long.MaxValue, max = long.MinValue, sum = 0;
            foreach (var t in times)
            {
                if (t < min) min = t;
                if (t > max) max = t;
                sum += t;
            }
            float avg = times.Count > 0 ? (float)sum / times.Count : 0f;
            UnityEngine.Debug.Log($"Table generation for {rowCount} rows: min={min} ms, max={max} ms, avg={avg:F2} ms over {REPEAT_COUNT} runs");
        }
        
        private void InitializeTableControl(int rowCount)
        {
            var tableAttributes = new TableAttributes()
            {
                tableType = TableType.Dynamic,
                columnReorderMode = TableReorderMode.ExplicitReorder,
                rowReorderMode = TableReorderMode.ExplicitReorder,
                columnHeaderVisibility = TableHeaderVisibility.ShowHeaderName,
                rowHeaderVisibility = TableHeaderVisibility.ShowHeaderName,
            };

            VisualElement rootVisualElement = new VisualElement();

            
            TableMetadata metadata = CreateTable(rowCount);
            var table = TableMetadataManager.GetTable(metadata);
            
            var times = new List<long>();
            for (int i = 0; i < REPEAT_COUNT; i++)
            {
                TableControl tableControl = new TableControl(rootVisualElement, tableAttributes, null, null, null);
                rootVisualElement.Add(tableControl);
                
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                tableControl.SetTable(table, metadata);
                stopwatch.Stop();
                times.Add(stopwatch.ElapsedMilliseconds);
                
                rootVisualElement.Remove(tableControl);
            }

            long min = long.MaxValue, max = long.MinValue, sum = 0;
            foreach (var t in times)
            {
                if (t < min) min = t;
                if (t > max) max = t;
                sum += t;
            }
            float avg = times.Count > 0 ? (float)sum / times.Count : 0f;
            UnityEngine.Debug.Log($"Table control initialization for {rowCount} rows: min={min} ms, max={max} ms, avg={avg:F2} ms over {REPEAT_COUNT} runs");
        }
        
        private TableMetadata CreateTable(int rowCount)
        {
            var metadata = TableMetadataManager.LoadMetadata($"PerfTable_{rowCount}", PathUtil.GetAndCreateTestDataFolder());
            if (metadata != null)
                return metadata;
            
            var guids = GetOrCreateTestScriptableObjects(rowCount);
            return TableMetadataManager.GetMetadata(guids, $"PerfTable_{rowCount}", PathUtil.GetAndCreateTestDataFolder());
        }

        [Test]
        public void TableGeneration_Performance_10_Rows()
        {
            GenerateTable(10);
        }

        [Test]
        public void TableGeneration_Performance_100_Rows()
        {
            GenerateTable(100);
        }
        
        [Test]
        public void TableGeneration_Performance_500_Rows()
        {
            GenerateTable(500);
        }

        [Test]
        public void TableGeneration_Performance_1000_Rows()
        {
            GenerateTable(1000);
        }

        [Test]
        public void TableGeneration_Performance_2000_Rows()
        {
            GenerateTable(2000);
        }
        
        [Test]
        public void TableControl_Initialization_Performance_10_Rows()
        {
            InitializeTableControl(10);
        }
        
        [Test]
        public void TableControl_Initialization_Performance_100_Rows()
        {
            InitializeTableControl(100);
        }
        
        [Test]
        public void TableControl_Initialization_Performance_500_Rows()
        {
            InitializeTableControl(500);
        }
        
        [Test]
        public void TableControl_Initialization_Performance_1000_Rows()
        {
            InitializeTableControl(1000);
        }
        
        [Test]
        public void TableControl_Initialization_Performance_2000_Rows()
        {
            InitializeTableControl(2000);
        }
    }
} 