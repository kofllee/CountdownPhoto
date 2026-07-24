using System;
using System.Collections.Generic;
using UnityEngine;

namespace _PhotoCountdown.Gameplay.Levels
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Photo Countdown/Level Catalog")]
    public class LevelCatalog : ScriptableObject
    {
        [SerializeField] private LevelDefinition[] _levels;

        public IReadOnlyList<LevelDefinition> Levels => _levels;

        public LevelDefinition GetAt(int index)
        {
            if (index < 0 || index >= _levels.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _levels[index];
        }

        public int IndexOf(LevelDefinition level)
        {
            return Array.IndexOf(_levels, level);
        }

        public LevelDefinition GetNext(LevelDefinition level)
        {
            int index = IndexOf(level);

            if (index < 0 || index + 1 >= _levels.Length)
                return null;

            return _levels[index + 1];
        }

        public void Validate()
        {
            if (_levels == null || _levels.Length == 0)
                throw new InvalidOperationException($"{name} has no levels.");

            HashSet<string> ids = new HashSet<string>();
            HashSet<string> scenes = new HashSet<string>();

            foreach (LevelDefinition level in _levels)
            {
                if (level == null)
                    throw new InvalidOperationException($"{name} contains a missing level.");

                level.Validate();

                if (!ids.Add(level.Id))
                    throw new InvalidOperationException($"{name} contains duplicate id {level.Id}.");

                if (!scenes.Add(level.SceneName))
                    throw new InvalidOperationException($"{name} contains duplicate scene {level.SceneName}.");
            }
        }
    }
}