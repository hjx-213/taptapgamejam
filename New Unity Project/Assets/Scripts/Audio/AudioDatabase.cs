
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    public class AudioDatabase : MonoBehaviour
    {
        [Serializable]
        public class BGMEntry
        {
            public string id;
            public AudioClip clip;
        }

        [Serializable]
        public class SFXEntry
        {
            public string id;
            public AudioClip clip;
        }

        [Header("背景音乐")]
        [SerializeField] private BGMEntry[] bgmEntries;

        [Header("音效")]
        [SerializeField] private SFXEntry[] sfxEntries;

        private Dictionary<string, AudioClip> bgmMap;
        private Dictionary<string, AudioClip> sfxMap;

        private void Awake()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            bgmMap = new Dictionary<string, AudioClip>(
                StringComparer.OrdinalIgnoreCase);

            sfxMap = new Dictionary<string, AudioClip>(
                StringComparer.OrdinalIgnoreCase);

            foreach (BGMEntry entry in bgmEntries)
            {
                if (entry != null &&
                    !string.IsNullOrWhiteSpace(entry.id) &&
                    entry.clip != null)
                {
                    bgmMap[entry.id.Trim()] = entry.clip;
                }
            }

            foreach (SFXEntry entry in sfxEntries)
            {
                if (entry != null &&
                    !string.IsNullOrWhiteSpace(entry.id) &&
                    entry.clip != null)
                {
                    sfxMap[entry.id.Trim()] = entry.clip;
                }
            }
        }

        public bool TryGetBGM(string id, out AudioClip clip)
        {
            clip = null;

            return !string.IsNullOrWhiteSpace(id) &&
                   bgmMap != null &&
                   bgmMap.TryGetValue(id.Trim(), out clip);
        }

        public bool TryGetSFX(string id, out AudioClip clip)
        {
            clip = null;

            return !string.IsNullOrWhiteSpace(id) &&
                   sfxMap != null &&
                   sfxMap.TryGetValue(id.Trim(), out clip);
        }
    }
}
