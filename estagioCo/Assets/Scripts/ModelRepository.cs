using System.Collections.Generic;
using UnityEngine;

public class ModelRepository : MonoBehaviour
{
    // 1) Singleton Instance for easy global access
    public static ModelRepository Instance { get; private set; }

    [System.Serializable]
    public struct Entry
    {
        public string key;        // must match cardData.modelPrefabName
        public GameObject prefab; // drag your prefab here
    }

    [Tooltip("List each model key & its corresponding prefab here")]
    [SerializeField] private Entry[] entries;

    // internal lookup
    private Dictionary<string, GameObject> dict;

    void Awake()
    {
        // enforce singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // build dictionary
        dict = new Dictionary<string, GameObject>();
        foreach (var e in entries)
        {
            if (!string.IsNullOrEmpty(e.key) && e.prefab != null)
                dict[e.key] = e.prefab;
        }
    }

    /// <summary>
    /// Returns the prefab associated with this key, or null if none.
    /// </summary>
    public GameObject GetPrefab(string key)
    {
        if (dict.TryGetValue(key, out var prefab))
            return prefab;
        Debug.LogWarning($"ModelRepository: no prefab found for key '{key}'");
        return null;
    }
}
