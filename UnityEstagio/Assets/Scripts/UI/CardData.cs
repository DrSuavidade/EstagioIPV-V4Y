using UnityEngine;

[System.Serializable]
public class CardData
{
    [Header("Visual")]
    public Sprite image; // A imagem/sprite do imóvel

    [Header("Informação Básica")]
    public string name; // Nome do imóvel
    public string streetName;
    public string location; // Lugar (ex.: “Lisboa, Portugal”)
    public float price; // Preço (ex.: 250000.0f)
    public string type; // Tipo (ex.: “Moradia T3”)
    public string size; // Tamanho textual (ex.: “200 m²”)
    public int area; // Outra medida textual
    public bool isFavorite; // Se está assinalado como favorito

    [Header("Descrição")]
    [TextArea]
    public string description; // Texto para a aba “Descrição”

    [Header("Detalhes")]
    [TextArea]
    public string detailsLeft; // Texto da coluna esquerda em “Detalhes”

    [TextArea]
    public string detailsRight; // Texto da coluna direita em “Detalhes”

    [Header("Coordinates")]
    public float latitude;   // ← NOVOS
    public float longitude;
    public string scanImageName;
    public string modelPrefabName;
}
