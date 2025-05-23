[System.Serializable]

public class CrewCardSaveData
{
    public CrewRace race;
    public string name;
    public bool isPurchased;

    public CrewCardSaveData(CrewRace race, string name)
    {
        this.race = race;
        this.name = name;
        this.isPurchased = false;
    }
}
