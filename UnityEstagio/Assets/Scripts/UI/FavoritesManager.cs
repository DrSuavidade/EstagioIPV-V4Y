// Assets/Scripts/UI/FavoritesManager.cs
using System.Collections.Generic;
using UnityEngine;

public class FavoritesManager : MonoBehaviour
{
    public static FavoritesManager Instance { get; private set; }

    // Agora guardamos referências a CardData, não strings
    private HashSet<CardData> favorites = new HashSet<CardData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Marca/desmarca este CardData em específico.
    /// </summary>
    public void SetFavorite(CardData data, bool isFav)
    {
        if (isFav)
            favorites.Add(data);
        else
            favorites.Remove(data);
    }

    /// <summary>
    /// Só retorna true se for *exatamente* essa instância que foi marcada.
    /// </summary>
    public bool IsFavorite(CardData data)
    {
        return favorites.Contains(data);
    }

    public IEnumerable<CardData> Favorites => favorites;
}
