using System;
using System.Collections.Generic;
using UnityEngine;

public class EnergyController
{
    public int energy;
    public int stash;
    public const int MAX_ENERGY = 12;
    public Action<int> OnEnergyChanged;
    public Action<int> OnStashChanged;
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
        energy = Mathf.Min(energy, MAX_ENERGY);
        OnEnergyChanged?.Invoke(energy);
    }
    public void DecreaseEnergy(int amount)
    {
        if (stash >= amount)
            stash -= amount;
        else
        {
            amount -= stash;
            stash = 0;
            energy = Mathf.Max(0, energy - amount);
        }

        OnEnergyChanged?.Invoke(energy);
        OnStashChanged?.Invoke(stash);
    }

    public bool HasEnoughEnergy(int amount) => stash + energy >= amount;
    public void MoveEnergyToStash()
    {
        stash += energy;
        //stash = Mathf.Min(stash, MAX_ENERGY); // Ako stash ima limit
        energy = 0;

        OnEnergyChanged?.Invoke(energy);
        OnStashChanged?.Invoke(stash);
    }

    public void ResetEnergy()
    {
        energy = MAX_ENERGY;
        OnEnergyChanged?.Invoke(energy);
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




