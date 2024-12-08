public class EnergyController
{
    public int energy;
    public int stash;
    public const int MAX_ENERGY = 12;

    public EnergyController() 
    { 
        energy = MAX_ENERGY; 
    }

    public EnergyController(EnergyData energyData)
    {
        energy = energyData.energy;
        stash = energyData.stash;
    }

    public void IncreaseEnergy(int amount)
    {
        energy += amount;
        energy = energy >= MAX_ENERGY ? MAX_ENERGY : energy;
    }
    public void DecreaseEnergy(int amount)
    {
        energy -= amount;
    }

    public bool CanDecreaseEnergy(int amount)
    {
        return energy >= amount;
    }
    public void MoveEnergyToStash()
    {
        stash += energy;
        energy = 0;
    }

    public void ResetEnergy()
    {
        energy = MAX_ENERGY;
    }

    public void GetEnergyFromStash(int amount)
    {
        if(stash < amount)
        {
            amount -= stash;
            stash = 0;
            DecreaseEnergy(amount);
        }
        else
            stash -= amount;
    }

    public bool CanGetEnergyFromStash(int amount)
    {
        return stash > 0 && stash + energy >= amount;
    }

    public EnergyData GetEnergyData()
    {
        return new EnergyData()
        { 
            energy = energy,
            stash = stash
        };

    }
}




