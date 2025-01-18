using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public Action<Game> OnGameCreated;
    public List<PlayerUI> players = new List<PlayerUI>();
    public RoundUI RoundUI;

    private void Start()
    {
        OnGameCreated += InitUI;
    }

    private void InitUI(Game game)
    {
        players[0].Initialize(game.players[0]);
        players[1].Initialize(game.players[1]);
        RoundUI.Init(game);
    }
}
