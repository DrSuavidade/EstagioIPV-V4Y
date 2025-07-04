// Assets/Scripts/Data/CatalogJson.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CatalogJson
{
    public List<SectionJson> sections;
}

[Serializable]
public class SectionJson
{
    public string title;
    public List<CardJson> cards;
}

[Serializable]
public class CardJson
{
    public string name;
    public string streetName;
    public string location;
    public float price;
    public string type;
    public string size;
    public int area;
    public string description;
    public string detailsLeft;
    public string detailsRight;
    public string imageName;
    public float latitude;
    public float longitude;
    public string scanImageName;
    public string modelPrefabName;
}
