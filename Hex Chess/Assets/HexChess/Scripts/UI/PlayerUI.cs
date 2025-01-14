using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerEnergy;
    [SerializeField] private TextMeshProUGUI playerStash;
    public void Initialize(Player player)
    {
        playerName.text = $"Name: {player.clientId}";
        playerEnergy.text = $"Energy: {player.energyController.energy}";
        playerStash.text = $"Stash: {player.energyController.stash}";

        player.energyController.OnEnergyChanged += (currentEnergy) => playerEnergy.text = $"Energy: {currentEnergy}";
        player.energyController.OnStashChanged += (currentStash) => playerStash.text = $"Stash: {currentStash}";
    }
}
