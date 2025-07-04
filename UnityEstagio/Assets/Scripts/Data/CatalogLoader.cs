// Assets/Scripts/Data/CatalogLoader.cs
using System.IO;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

public class CatalogLoader : MonoBehaviour
{
    [Tooltip("Sem extensão, ex: 'catalog' para StreamingAssets/catalog.json")]
    public string jsonFileName = "catalog";

    [HideInInspector]
    public CatalogJson rawData;

    // estrutura final: seções com List<CardData>
    public System.Collections.Generic.List<CatalogController.Section> sections;

    public event Action OnCatalogLoaded;

    // We use Start() as a coroutine, so we can yield on Android's UnityWebRequest.
    IEnumerator Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName + ".json");
        string json = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android the StreamingAssets folder is packed into the .apk
        using (var www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError($"Failed loading '{path}': {www.error}");
            else
                json = www.downloadHandler.text;
        }
#else
        // In Editor / standalone we can read directly
        if (!File.Exists(path))
        {
            Debug.LogError($"File not found at '{path}'");
        }
        else
        {
            json = File.ReadAllText(path);
        }
#endif

        if (!string.IsNullOrEmpty(json))
        {
            rawData = JsonUtility.FromJson<CatalogJson>(json);
            BuildSections();
            OnCatalogLoaded?.Invoke();
        }
        yield break;
    }
    void BuildSections()
    {
        sections = rawData.sections
            .Select(sJson =>
            {
                var sec = new CatalogController.Section
                {
                    title = sJson.title,
                    cards = sJson.cards.Select(cj =>
                    {
                        var cd = new CardData
                        {
                            name = cj.name,
                            streetName = cj.streetName,
                            location = cj.location,
                            price = cj.price,
                            type = cj.type,
                            size = cj.size,
                            area = cj.area,
                            description = cj.description,
                            detailsLeft = cj.detailsLeft,
                            detailsRight = cj.detailsRight,
                            image = Resources.Load<Sprite>("Images/Houses/" + cj.imageName),
                            latitude = cj.latitude,
                            longitude = cj.longitude,
                            scanImageName    = cj.scanImageName,
                            modelPrefabName  = cj.modelPrefabName

                            // isFavorite fica false por defeito
                        };
                        return cd;
                    }).ToList()
                };
                return sec;
            }).ToList();
    }
}
