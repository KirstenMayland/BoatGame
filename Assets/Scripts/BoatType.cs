using UnityEngine;

[System.Serializable]
public class BoatType
{
    public string name;
    public float draft; // How deep the boat sits in water
    public float safetyMargin = 2f; // Extra clearance needed
    
    // Constructor for easy creation
    public BoatType(string boatName, float boatDraft, float margin = 2f)
    {
        name = boatName;
        draft = boatDraft;
        safetyMargin = margin;
    }
    
    // Default constructor for Unity serialization
    public BoatType() { }
    
    public float GetRequiredDepth()
    {
        return draft + safetyMargin;
    }
}

public class BoatTypeManager : MonoBehaviour
{
    [Header("Available Boat Types")]
    public BoatType[] boatTypes = new BoatType[]
    {
        new BoatType("Sailboat", 1.5f, 2f),
        new BoatType("Motorboat", 0.8f, 1.5f),
        new BoatType("Cargo Ship", 12f, 3f),
        new BoatType("Submarine", 8f, 2f)
    };
    
    // Helper methods
    public BoatType GetBoatTypeByName(string boatName)
    {
        foreach (BoatType boat in boatTypes)
        {
            if (boat.name == boatName)
                return boat;
        }
        return null;
    }
    
    public BoatType GetBoatTypeByIndex(int index)
    {
        if (index >= 0 && index < boatTypes.Length)
            return boatTypes[index];
        return null;
    }
}