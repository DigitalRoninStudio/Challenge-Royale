using System.Collections.Generic;

public class RoundController
{
    public int round;
    public Team startRoundTeam;
    public Team teamWithInitiation;

    public RoundController() { round = 0; }
    public RoundController(RoundData roundData)
    {
        round = roundData.round;
        startRoundTeam = roundData.startRoundTeam;
        teamWithInitiation = roundData.teamWithInitiation;
    }

    public void EndRound(List<Player> players, RandomGenerator random)
    {
        bool shouldRoll = false;
        foreach (Player player in players)
        {
            if (player.energyController.energy > 0)
            {
                shouldRoll = true;
                break;
            }
        }

        if (shouldRoll)
        {
            int roll = random.NextInt(1, EnergyController.MAX_ENERGY);
            foreach (Player player in players) 
            {
                if (player.energyController.energy == 0) continue;

                if (player.energyController.energy <= roll)
                    player.energyController.MoveEnergyToStash();
                else
                    player.energyController.ClearEnergy();
            }
        }

        StartNewRound();
    }
    public void SetFirstRound(List<Player> players, RandomGenerator random)
    {
        int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int player1 = random.NextInt(1, EnergyController.MAX_ENERGY);
            int player2 = random.NextInt(1, EnergyController.MAX_ENERGY);

            if (player1 != player2)
            {
                startRoundTeam = player1 > player2 ? players[0].team : players[1].team;
                teamWithInitiation = startRoundTeam;
                round++;
                return;
            }
        }

        int winRand = random.NextInt(EnergyController.MAX_ENERGY / 2, EnergyController.MAX_ENERGY);
        int loseRand = random.NextInt(1, EnergyController.MAX_ENERGY / 2 - 1);

        startRoundTeam = random.NextInt(0, 1) == 0 ? players[0].team : players[1].team;
        teamWithInitiation = startRoundTeam;
        round++;
    }
    public void StartNewRound()
    {
        startRoundTeam = startRoundTeam == Team.GOOD_BOYS ? Team.BAD_BOYS : Team.GOOD_BOYS;
        teamWithInitiation = startRoundTeam;
        round++;
    }

    public void SwitchInitiation()
    {
        teamWithInitiation = teamWithInitiation == Team.GOOD_BOYS ? Team.BAD_BOYS : Team.GOOD_BOYS;
    }

    public RoundData GetRoundData()
    {
        return new RoundData() 
        { 
            round = round ,
            startRoundTeam = startRoundTeam,
            teamWithInitiation = teamWithInitiation
        }; 
    }
}
