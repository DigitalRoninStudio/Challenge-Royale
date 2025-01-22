using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RoundUI : MonoBehaviour
{
    [SerializeField] private Button EndRound;
    [SerializeField] private Button SwitchInitiation;
    [SerializeField] private TextMeshProUGUI RoundCounter;
    [SerializeField] private TextMeshProUGUI Player1OnTurn;
    [SerializeField] private TextMeshProUGUI Player2OnTurn;

    public void Init(Game game)
    {
        RoundCounter.text = $"ROUND\n{game.roundController.round}";
        if (game.roundController.teamWithInitiation == Team.GOOD_BOYS)
        {
            Player1OnTurn.gameObject.SetActive(true);
            Player2OnTurn.gameObject.SetActive(false);
        }
        else
        {
            Player1OnTurn.gameObject.SetActive(false);
            Player2OnTurn.gameObject.SetActive(true);
        }

        game.roundController.OnChangeRound += (round) => RoundCounter.text = $"ROUND\n{round}";
        game.roundController.OnChangeInitiation += (team) =>
        {
            if (team == Team.GOOD_BOYS)
            {
                Player1OnTurn.gameObject.SetActive(true);
                Player2OnTurn.gameObject.SetActive(false);
            }else
            {
                Player1OnTurn.gameObject.SetActive(false);
                Player2OnTurn.gameObject.SetActive(true);
            }
        };

        EndRound.onClick.AddListener(() =>
        {
            if(Client.IsClient && LocalPlayer.Instance.CanLocalPlayerDoAction())
            {
                LocalPlayer.Instance.EndRound();
            }
        });

        SwitchInitiation.onClick.AddListener(() =>
        {
            if (Client.IsClient && LocalPlayer.Instance.CanLocalPlayerDoAction() && !game.roundController.endTurnCalled)
            {
                LocalPlayer.Instance.HandOverTheInitiative();
            }
        });
    }
}
