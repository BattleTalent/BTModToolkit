using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossLink.EditorTools
{
    [Serializable]
    public sealed class PathFinderPreviewParameters
    {
        public const float DefaultMinRegionSize = 2;
        public const float DefaultCellSize = 0.25f;
        public const float DefaultMaxSlope = 30;

        public float walkableClimb = 0.5f;
        public bool rasterizeTerrain = true;
        public bool rasterizeMeshes = true;
        public bool rasterizeColliders = true;
        public float minRegionSize = DefaultMinRegionSize;
        public float cellSize = DefaultCellSize;
        public float maxSlope = DefaultMaxSlope;

        public PathFinderPreviewParameters Copy()
        {
            return (PathFinderPreviewParameters)MemberwiseClone();
        }

        public bool Matches(PathFinderPreviewParameters other)
        {
            return other != null && walkableClimb.Equals(other.walkableClimb) &&
                rasterizeTerrain == other.rasterizeTerrain &&
                rasterizeMeshes == other.rasterizeMeshes &&
                rasterizeColliders == other.rasterizeColliders &&
                minRegionSize.Equals(other.minRegionSize) && cellSize.Equals(other.cellSize) &&
                maxSlope.Equals(other.maxSlope);
        }

        public string ValidationError()
        {
            if (!IsFinite(walkableClimb) || walkableClimb < 0)
                return "walkableClimb must be a finite value greater than or equal to zero.";
            if (!IsFinite(minRegionSize) || minRegionSize < 0)
                return "minRegionSize must be a finite value greater than or equal to zero.";
            if (!IsFinite(cellSize) || cellSize <= 0)
                return "cellSize must be a finite value greater than zero.";
            if (!IsFinite(maxSlope) || maxSlope < 0 || maxSlope > 90)
                return "maxSlope must be a finite angle between 0 and 90 degrees.";
            return null;
        }

        public void ResetAdditionalDefaults()
        {
            minRegionSize = DefaultMinRegionSize;
            cellSize = DefaultCellSize;
            maxSlope = DefaultMaxSlope;
        }

        public static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    [FilePath("ProjectSettings/PathFinderPreviewSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class PathFinderPreviewSettings : ScriptableSingleton<PathFinderPreviewSettings>
    {
        [Serializable]
        private sealed class Entry
        {
            public string globalId;
            public PathFinderPreviewParameters parameters;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();
        [SerializeField] private int settingsVersion;

        private void EnsureCurrentVersion()
        {
            if (settingsVersion >= 1)
                return;
            foreach (Entry entry in entries)
            {
                if (entry.parameters != null)
                    entry.parameters.ResetAdditionalDefaults();
            }
            settingsVersion = 1;
            Save(true);
        }

        public static bool TryGetKey(BoxCollider range, out string key)
        {
            key = null;
            if (range == null || !range.gameObject.scene.IsValid() ||
                string.IsNullOrEmpty(range.gameObject.scene.path) || EditorUtility.IsPersistent(range))
                return false;

            GlobalObjectId id = GlobalObjectId.GetGlobalObjectIdSlow(range);
            if (id.identifierType == 0 || id.targetObjectId == 0)
                return false;
            key = id.ToString();
            return true;
        }

        public PathFinderPreviewParameters Load(string key)
        {
            EnsureCurrentVersion();
            Entry entry = entries.Find(candidate => candidate.globalId == key);
            return entry != null && entry.parameters != null
                ? entry.parameters.Copy()
                : new PathFinderPreviewParameters();
        }

        public void Store(string key, PathFinderPreviewParameters parameters)
        {
            if (string.IsNullOrEmpty(key) || parameters == null || parameters.ValidationError() != null)
                return;

            EnsureCurrentVersion();
            Entry entry = entries.Find(candidate => candidate.globalId == key);
            if (entry == null)
            {
                entry = new Entry { globalId = key };
                entries.Add(entry);
            }
            entry.parameters = parameters.Copy();
            Save(true);
        }
    }
}
