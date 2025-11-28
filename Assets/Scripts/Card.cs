using System;
using UnityEngine;
using GameData;

[RequireComponent(typeof(CardRenderer))]
public class Card : MonoBehaviour {
    
    

    #region Data Definitions

    public enum RankType {
        First, Second, Third, Fourth
    }
    public enum BrightType {
        None, Plain, Moon, Sakura, RainMan
    }
    public enum AnimalType {
        None, Plain, Boar, Deer, Butterfly, Sake
    }
    public enum RibbonType {
        None, Red, Text, Blue
    }
    public enum CardType {
        Plain, Bright, Animal, Ribbon
    }

    #endregion

    #region Serialized Fields (Unity Inspector)
    [Tooltip("Texture lookup table to retrieve textures from")]
    [SerializeField] private TextureAtlas textureAtlas;
    [Tooltip("Mesh renderer of the card")]
    [SerializeField] private MeshRenderer meshRenderer;
    #endregion
    #region Public Members (Inspector Hidden)
    [HideInInspector] public event CardSelectDelegate OnCardSelect;
    #endregion
    
    #region Card Data Properties
    
    /// <summary>
    /// The card's data represented in packed binary
    /// </summary>
    public int BinaryData { get; private set; }
    
    /// <summary>
    /// Unique ID consisting of the card's rank and month.
    /// </summary>
    public int UniqueID { get; private set; }
    
    /// <summary>
    /// The value that determines which card within a month this card is specifically.
    /// </summary>
    public RankType Rank { get; private set; }

    /// <summary>
    /// The month this card is a part of.
    /// </summary>
    public MonthType Month { get; private set; }

    /// <summary>
    /// The type of bright card this card is.
    /// </summary>
    public BrightType Bright { get; private set; }

    /// <summary>
    /// The type of animal card this card is.
    /// </summary>
    public AnimalType Animal { get; private set; }

    /// <summary>
    /// The type of ribbon card this card is.
    /// </summary>
    public RibbonType Ribbon { get; private set; }

    /// <summary>
    /// Returns true if the card is a brght card.
    /// </summary>
    public bool IsBright => Bright != BrightType.None;
    
    /// <summary>
    /// Returns true if this card is an animal card.
    /// </summary>
    public bool IsAnimal => Animal != AnimalType.None;

    /// <summary>
    /// Returns true if this card is a ribbon card.
    /// </summary>
    public bool IsRibbon => Ribbon != RibbonType.None;

    /// <summary>
    /// Returns the name of the month of the card.
    /// </summary>
    /// <remarks>
    /// Same as Card.Month.ToString();
    /// </remarks>
    public string MonthName => Month.ToString();

    /// <summary>
    /// Returns the name of the animal of the card if one exists, otherwise returns an empty string.
    /// </summary>
    /// <remarks>
    /// Same as Card.Animal.ToString();
    /// </remarks>
    public string AnimalName => Animal == AnimalType.None ? "" : Animal.ToString();

    /// <summary>
    /// Returns the name of the bright type of the card if one exists, otherwise returns an empty string.
    /// </summary>
    /// <remarks>
    /// Same as Card.Bright.ToString();
    /// </remarks>
    public string BrightName => Bright == BrightType.None ? "" : Bright.ToString();
    
    /// <summary>
    /// Returns the name of the ribbon type of the card if one exists, otherwise returns an empty string.
    /// </summary>
    /// <remarks>
    /// Same as Card.Ribbon.ToString();
    /// </remarks>
    public string RibbonName => Ribbon == RibbonType.None ? "" : Ribbon.ToString();

    /// <summary>
    /// The special data this card contains.
    /// </summary>
    public CardType CardTypeData {
        get {
            if (IsBright) return CardType.Bright;
            else if (IsAnimal) return CardType.Animal;
            else if (IsRibbon) return CardType.Ribbon;
            else return CardType.Plain;
        }
    }

    #endregion
    
    #region Rendering Data
    private MaterialPropertyBlock material_properties;
    #endregion

    /// <summary>
    /// Set the values of the card using its network-ready binary data.
    /// </summary>
    /// <remarks>
    /// Recommended to use <see cref="LoadCardData">loadCardData</see> instead as SetBinaryData may have unintended consequences if the data is invalid.
    /// </remarks>
    /// <param name="data">The card's binary data. Will be of the type ushort when networked.</param>
    /// <exception cref="ArgumentOutOfRangeException">A card value is corrupted and out of range.</exception>
    public void SetBinaryData(int data) {
        // Set the basic info of the card
        BinaryData = data;
        UniqueID = data & 0b111111;

        // Get the values used with enum types for checking later
        int v_rank = data & 0b11;
        int v_month = (data >> 2) & 0b1111;
        int v_bright = (data >> 6) & 0b11;
        int v_animal = (data >> 8) & 0b111;
        int v_ribbon = (data >> 11) & 0b11;

        // Throw exceptions if any values with associated enums are out of the enums' ranges
        if (v_rank >= 4) throw new ArgumentOutOfRangeException($"Rank value {v_rank} is out of range. Binary Data: {data:b16}");
        if (v_month >= 12) throw new ArgumentOutOfRangeException($"Month value {v_month} is out of range. Binary Data: {data:b16}");
        if (v_bright >= 4) throw new ArgumentOutOfRangeException($"Bright value {v_bright} is out of range. Binary Data: {data:b16}");
        if (v_animal >= 6) throw new ArgumentOutOfRangeException($"Animal value {v_animal} is out of range. Binary Data: {data:b16}");
        if (v_ribbon >= 4) throw new ArgumentOutOfRangeException($"Ribbon value {v_ribbon} is out of range. Binary Data: {data:b16}");
        
        // Assign the enum variables their values
        Rank = (RankType)v_rank;
        Month = (MonthType)v_month;
        Bright = (BrightType)v_bright;
        Animal = (AnimalType)v_animal;
        Ribbon = (RibbonType)v_ribbon;

        // Create and use the material property block
        material_properties ??= new MaterialPropertyBlock();
        material_properties.SetTexture("_BaseMap", textureAtlas.GetTexture(UniqueID));
        meshRenderer.SetPropertyBlock(material_properties);
        OnValidate();
    }

    /// <summary>
    /// Load the card data directly from a cardData scriptable object.
    /// </summary>
    /// <param name="cardData">Scriptable object containing the card data.</param>
    public void LoadCardData(CardData cardData) {
        BinaryData = cardData.BinaryData;
        UniqueID = cardData.UniqueID;
        Rank = cardData.Rank;
        Month = cardData.Month;
        Bright = cardData.Bright;
        Animal = cardData.Animal;
        Ribbon = cardData.Ribbon;
        // Create and use the material property block
        material_properties ??= new MaterialPropertyBlock();
        material_properties.SetTexture("_BaseMap", textureAtlas.GetTexture(UniqueID));
        meshRenderer.SetPropertyBlock(material_properties);
        OnValidate();
    }


    /// <summary>
    /// Log the debug info of the card to the console.
    /// </summary>
    public void printCardDebugInfo()
    {
        Debug.Log(
            $"Binary data: {BinaryData}\n" +
            $"Unique ID: {UniqueID}\n" +
            $"Rank: {Rank}\n" +
            $"Month: {Month}, {Month.ToString()}\n" +
            $"Bright: {Bright}, {Bright.ToString()}\n" +
            $"Animal: {Animal}, {Animal.ToString()}\n" +
            $"Ribbon: {Ribbon}, {Ribbon.ToString()}"
        );
    }

#if UNITY_EDITOR
    [SerializeField, TextArea] private string debugInfo;
    #endif

    private void OnValidate()
    {
    #if UNITY_EDITOR
        debugInfo =
            $"Binary data: {BinaryData}\n" +
            $"Unique ID: {UniqueID}\n" +
            $"Rank: {Rank}\n" +
            $"Month: {MonthName}\n" +
            $"Animal: {AnimalName}\n" +
            $"Bright: {BrightName}\n" +
            $"Ribbon: {RibbonName}";
    #endif
    }

    public void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<KoiKoiGameManager>();
    }

    public void OnMouseDown()
    {
        OnCardSelect(this);
        /*
        
        */
    }
}
