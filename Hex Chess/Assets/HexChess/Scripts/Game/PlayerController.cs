using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerController : MonoBehaviour, PlayerInGameController.IBasicStateActions, PlayerInGameController.IEntitySelectedStateActions
{
    public PlayerInGameController inputController;

    public GameManager gameManager => GameManager.Instance;
    public IPlayerState BasicState { get; private set; }
    public IPlayerState EntitySelectedState { get; private set; }

    private IPlayerState currentState;

    public Action OnRefreshFields;
    public Action<MovementBehaviour, Tile> OnShowMovementFields;

    private void Awake()
    {
        inputController = new PlayerInGameController();
        inputController.BasicState.SetCallbacks(this);
        inputController.EntitySelectedState.SetCallbacks(this);

        BasicState = new BasicState();
        EntitySelectedState = new EntitySelectedState();

        currentState = BasicState;
        currentState.EnterState(this);
    }
    private void OnEnable()
    {
        inputController.Enable();
    }

    private void OnDisable()
    {
        inputController.Disable();
    }
    public void TransitionToState(IPlayerState newState)
    {
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }
    #region BasicAction
    public void OnScreenPosition(InputAction.CallbackContext context)
    {
        if (currentState is BasicState basicState)
            basicState.OnScreenPosition(context, this);
        else if (currentState is EntitySelectedState entitySelectedState)
            entitySelectedState.OnScreenPosition(context, this);
    }

    public void OnOnSelectEntity(InputAction.CallbackContext context)
    {

        if (currentState is BasicState basicState)
            basicState.OnSelectEntity(context, this);
        else if (currentState is EntitySelectedState entitySelectedState)
            entitySelectedState.OnSelectEntity(context, this);
    }

    public void OnOnDeselectEntity(InputAction.CallbackContext context)
    {
        if(currentState is EntitySelectedState entitySelectedState)
            entitySelectedState.OnDeselectEntity(context, this);
    }
    #endregion

}
public interface IPlayerState
{
    void EnterState(PlayerController playerController);
    void ExitState(PlayerController playerController);
}

public class BasicState : IPlayerState
{
    public void EnterState(PlayerController playerController)
    {
        playerController.inputController.BasicState.Enable();
    }

    public void ExitState(PlayerController playerController)
    {
        playerController.OnRefreshFields?.Invoke();
        playerController.inputController.BasicState.Disable();
    }
    public void OnScreenPosition(InputAction.CallbackContext context, PlayerController playerController)
    {
        if (context.performed)
        {
            playerController.gameManager.mousePosition = context.ReadValue<Vector2>();
        }
    }

    public void OnSelectEntity(InputAction.CallbackContext context, PlayerController playerController)
    {
        if (context.performed)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            //move to method
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(playerController.gameManager.mousePosition);
            Tile tile = playerController.gameManager.game.map.OnHoverMapGetTile(worldPosition);
            List<Entity> entities = tile?.GetEntities() ?? new List<Entity>();

            if (tile != null)
            {
                foreach (Entity entity in entities)
                {
                    if(entity is Figure)
                    {
                        playerController.gameManager.selectedEntity = entity;
                        playerController.TransitionToState(playerController.EntitySelectedState);
                        return;
                    }
                }
            }
        }
    }
}

public class EntitySelectedState : IPlayerState
{
    public void EnterState(PlayerController playerController)
    {
        //hot fix
        MovementBehaviour movementBehaviour = playerController.gameManager.selectedEntity.GetBehaviour<MovementBehaviour>();
        Tile tile = playerController.gameManager.game.map.GetTile(playerController.gameManager.selectedEntity);
        if (movementBehaviour != null && tile != null)
            playerController.OnShowMovementFields?.Invoke(movementBehaviour, tile);

        playerController.inputController.EntitySelectedState.Enable();
    }
    public void ExitState(PlayerController playerController)
    {
        playerController.OnRefreshFields?.Invoke();
        playerController.inputController.EntitySelectedState.Disable();
    }

    public void OnScreenPosition(InputAction.CallbackContext context, PlayerController playerController)
    {
        if (context.performed)
        {
            playerController.gameManager.mousePosition = context.ReadValue<Vector2>();
        }
    }

    public void OnSelectEntity(InputAction.CallbackContext context, PlayerController playerController)
    {
        if (context.performed)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            //move to method
            Vector2 worldPosition = Camera.main.ScreenToWorldPoint(playerController.gameManager.mousePosition);
            Tile tile = playerController.gameManager.game.map.OnHoverMapGetTile(worldPosition);
            List<Entity> entities = tile?.GetEntities() ?? new List<Entity>();


            if (tile != null)
            {
                foreach (Entity entity in entities)
                {
                    if(entity is Figure)
                    {
                        if (playerController.gameManager.selectedEntity == entity) return;

                        playerController.gameManager.selectedEntity = entity;
                        playerController.TransitionToState(playerController.EntitySelectedState);
                        return;
                    }
                }
            }
        }
    }

    public void OnDeselectEntity(InputAction.CallbackContext context, PlayerController playerController)
    {
        if (context.performed)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            //hot fix
            //if enemy unit is there attack, maybe change this to attack entity

            playerController.gameManager.selectedEntity = null;
            playerController.TransitionToState(playerController.BasicState);
        }
    }
}


