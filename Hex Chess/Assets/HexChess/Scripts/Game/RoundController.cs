
using UnityEngine;

public class RoundController
{
    public int round;
    public Team startRoundTeam;
    public Team teamWithInitiation;
    public bool endTurnCalled;
    private Game game;

    public RoundController(Game game) { round = 0; this.game = game; }
    public RoundController(Game game, RoundData roundData)
    {
        round = roundData.Round;
        startRoundTeam = roundData.StartRoundTeam;
        teamWithInitiation = roundData.TeamWithInitiation;
        this.game = game;
    }

    public void EndRound()
    {
        bool shouldRoll = false;
        foreach (Player player in game.players)
        {
            if (player.energyController.energy > 0)
            {
                shouldRoll = true;
                break;
            }
        }

        if (shouldRoll)
        {
            int roll = game.randomGenerator.NextInt(1, EnergyController.MAX_ENERGY);
            foreach (Player player in game.players) 
            {
                if (player.energyController.energy == 0) continue;

                if (player.energyController.energy <= roll)
                    player.energyController.MoveEnergyToStash();

                player.energyController.ResetEnergy();
            }
        }

        endTurnCalled = false;
        StartNewRound();
    }
    public void SetFirstRound()
    {
        int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int player1Roll = game.randomGenerator.NextInt(1, EnergyController.MAX_ENERGY);
            int player2Roll = game.randomGenerator.NextInt(1, EnergyController.MAX_ENERGY);

            if (player1Roll != player2Roll)
            {
                startRoundTeam = player1Roll > player2Roll ? game.players[0].team : game.players[1].team;
                teamWithInitiation = startRoundTeam;
                round++;

                foreach (var player in game.players)
                    player.SetPlayerStateDependOnInitiation(teamWithInitiation);

                return;
            }
        }

        int winRand = game.randomGenerator.NextInt(EnergyController.MAX_ENERGY / 2, EnergyController.MAX_ENERGY);
        int loseRand = game.randomGenerator.NextInt(1, EnergyController.MAX_ENERGY / 2 - 1);

        startRoundTeam = game.randomGenerator.NextInt(0, 1) == 0 ? game.players[0].team : game.players[1].team;
        teamWithInitiation = startRoundTeam;
        round++;

        foreach (var player in game.players)
            player.SetPlayerStateDependOnInitiation(teamWithInitiation);
    }
    public void StartNewRound()
    {
        startRoundTeam = startRoundTeam == Team.GOOD_BOYS ? Team.BAD_BOYS : Team.GOOD_BOYS;
        teamWithInitiation = startRoundTeam;
        round++;

        foreach (var player in game.players)
            player.SetPlayerStateDependOnInitiation(teamWithInitiation);
    }

    public void SwitchInitiation()
    {
        teamWithInitiation = teamWithInitiation == Team.GOOD_BOYS ? Team.BAD_BOYS : Team.GOOD_BOYS;

        foreach (var player in game.players)
            player.SetPlayerStateDependOnInitiation(teamWithInitiation);

    }

    public void EndRoundAndSwitchInitiation()
    {
        SwitchInitiation();
        endTurnCalled = true;
    }

    public RoundData GetRoundData()
    {
        return new RoundData() 
        { 
            Round = round ,
            StartRoundTeam = startRoundTeam,
            TeamWithInitiation = teamWithInitiation
        }; 
    }
}
