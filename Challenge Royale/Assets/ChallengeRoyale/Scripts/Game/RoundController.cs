﻿
using Newtonsoft.Json;
using System;
using UnityEngine;

public class RoundController
{
    public int round;
    public Team startRoundTeam;
    public Team teamWithInitiation;
    public bool endTurnCalled;
    private Game game;
    public Action<int> OnChangeRound;
    public Action<Team> OnChangeInititation;
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
        Debug.Log("END ROUND");
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

                OnChangeInititation?.Invoke(teamWithInitiation);
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

        OnChangeInititation?.Invoke(teamWithInitiation);
    }
    public void StartNewRound()
    {
        startRoundTeam = startRoundTeam == Team.GOOD_BOYS ? Team.BAD_BOYS : Team.GOOD_BOYS;
        teamWithInitiation = startRoundTeam;
        round++;

        foreach (var player in game.players)
            player.SetPlayerStateDependOnInitiation(teamWithInitiation);

        OnChangeInititation?.Invoke(teamWithInitiation);
        OnChangeRound?.Invoke(round);
    }

    public void SwitchInitiation()
    {
        Debug.Log("Switch Initiation");
        teamWithInitiation = teamWithInitiation == Team.GOOD_BOYS ? Team.BAD_BOYS : Team.GOOD_BOYS;

        foreach (var player in game.players)
            player.SetPlayerStateDependOnInitiation(teamWithInitiation);

        OnChangeInititation?.Invoke(teamWithInitiation);

    }

    public void EndRoundAndSwitchInitiation()
    {
        Debug.Log("End Round And Switch Initiation");
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
public class RoundAction : INetAction, ILifecycleAction
{
    public bool EndRound;
    public bool SwitchInitiation; 
    public ActionType ActionType => ActionType.ROUND;
    public Action OnActionStart { get; set; } = () => { };
    public Action OnActionExecuted { get; set; } = () => { };
    public Action OnActionEnd { get; set; } = () => { };
    private Game _game;
    public RoundAction(Game game)
    {
        _game = game;
    }

    public void Enter()
    {
        OnActionStart?.Invoke();
    }
    public bool CanBeExecuted() => true;
    public void Execute()
    {
        Game game = GameManager.Instance.GetFirstMatch();

        if (EndRound && SwitchInitiation)
            game.roundController.EndRoundAndSwitchInitiation();
        else if (SwitchInitiation)
            game.roundController.SwitchInitiation();
        else if (EndRound)
            game.roundController.EndRound();

        Exit();
    }

    public void Exit()
    {
        OnActionEnd?.Invoke();
    }

    public string SerializeAction()
    {
        RoundActionData actionData = new RoundActionData()
        {
            EndRound = this.EndRound,
            SwitchInitiation = this.SwitchInitiation
        };
        ExecutedAction executedAction = new ExecutedAction()
        {
            roundActionData = actionData
        };
        _game.executedClientsActions.Add(executedAction);
        return JsonConvert.SerializeObject(actionData);
    }

    public void DeserializeAction(string actionJson)
    {
        RoundActionData actionData = JsonConvert.DeserializeObject<RoundActionData>(actionJson);
        EndRound = actionData.EndRound;
        SwitchInitiation = actionData.SwitchInitiation;

        ExecutedAction executedAction = new ExecutedAction()
        {
            roundActionData = actionData
        };
        _game.executedClientsActions.Add(executedAction);
    }
}