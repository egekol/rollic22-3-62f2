using System;
using System.Collections.Generic;
using System.IO;
using Core.Scripts.Lib.Utility;
using UnityEngine;

namespace Core.Scripts.Gameplay.Levels
{
    public interface ILevelProvider
    {
        IReadOnlyList<LevelDataSo> LevelDataList { get; }
    }

    public class LevelExporter : Singleton<LevelExporter>, ILevelProvider
    {
        [SerializeField] private List<LevelDataSo> _levelDataList;
        public IReadOnlyList<LevelDataSo> LevelDataList => _levelDataList;
        public const string LevelsCsvFolderPath = "Assets/Core/LevelCsv";
        public const string LevelsSoFolderPath = "Assets/Core/ScriptableObjects/LevelDataList";
        public const string LevelsDataName = "LevelData_{0}";

        public const int GridStartColumn = 3; // Column D (0-indexed: A=0, B=1, C=2, D=3)

        public static LevelDataSo ParseCsvToLevelData(string csvContent, string levelName, int levelIndex)
        {
            var levelData = ScriptableObject.CreateInstance<LevelDataSo>();
            levelData.LevelName = levelName;
            levelData.LevelIndex = levelIndex;
            levelData.Difficulty = LevelDifficultyType.Easy;

            var lines = csvContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0)
            {
                Debug.LogError($"CSV is empty for level: {levelName}");
                return null;
            }

            var tileList = new List<LevelTileData>();
            int maxColumns = 0;
            int rowCount = lines.Length;

            // Parse the grid from CSV
            for (int y = 0; y < lines.Length; y++)
            {
                var cells = ParseCsvLine(lines[y]);
                
                // Grid starts from column D (index 3)
                for (int x = GridStartColumn; x < cells.Length; x++)
                {
                    var cellValue = cells[x].Trim();
                    if (string.IsNullOrEmpty(cellValue))
                        continue;

                    if (int.TryParse(cellValue, out int tileTypeValue))
                    {
                        if (Enum.IsDefined(typeof(TileType), tileTypeValue))
                        {
                            var tileData = new LevelTileData
                            {
                                Type = (TileType)tileTypeValue,
                                Coordinates = new Vector2Int(x - GridStartColumn, y)
                            };
                            tileList.Add(tileData);

                            int gridX = x - GridStartColumn + 1;
                            if (gridX > maxColumns)
                                maxColumns = gridX;
                        }
                    }
                }
            }

            levelData.GridSize = new Vector2Int(maxColumns, rowCount);
            levelData.Tiles = tileList.ToArray();

            return levelData;
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            result.Add(currentField);

            return result.ToArray();
        }

        public static string[] GetAllCsvFiles()
        {
            if (!Directory.Exists(LevelsCsvFolderPath))
            {
                Debug.LogError($"CSV folder not found: {LevelsCsvFolderPath}");
                return Array.Empty<string>();
            }

            return Directory.GetFiles(LevelsCsvFolderPath, "*.csv");
        }

        public LevelDataSo GetLevelData(int levelIndex)
        {
            foreach (var levelData in _levelDataList)
            {
                if (levelData.LevelIndex == levelIndex)
                {
                    return levelData;
                }
            }

            if (levelIndex < _levelDataList.Count)
            {
                return _levelDataList[levelIndex];
            }
            
            return null;
        }
    }
}
