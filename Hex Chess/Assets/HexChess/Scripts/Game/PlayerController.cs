using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, PlayerInGameController.IBasicStateActions, PlayerInGameController.IEntitySelectedStateActions
{
    public PlayerInGameController inputController;

    public GameManager gameManager => GameManager.Instance;
    public IPlayerState BasicState { get; private set; }
    public IPlayerState EntitySelectedState { get; private set; }

    private IPlayerState currentState;

    public Action OnResetTiles;
    public Action<Entity> OnSelectEntity;

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
        Debug.Log("ENTER BASIC STATE");
        playerController.inputController.BasicState.Enable();
    }

    public void ExitState(PlayerController playerController)
    {
        Debug.Log("EXIT BASIC STATE");
        playerController.OnResetTiles?.Invoke();
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
        if (!context.performed || EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(playerController.gameManager.mousePosition);
        Tile tile = playerController.gameManager.game.map.OnHoverMapGetTile(worldPosition);

        if (tile == null)
            return;

        List<Entity> entities = tile.GetEntities() ?? new List<Entity>();

        if (entities.Count != 0)
            TrySelectEntityOnTile(playerController, entities);

    }
    private void TrySelectEntityOnTile(PlayerController playerController, List<Entity> entities)
    {
        foreach (Entity entity in entities)
        {
            if (entity is Figure)
            {
                playerController.gameManager.selectedEntity = entity;
                playerController.TransitionToState(playerController.EntitySelectedState);
                break;
            }
        }
    }
}

public class EntitySelectedState : IPlayerState
{
    public void EnterState(PlayerController playerController)
    {
        Debug.Log("ENTER ENTITY SELECTED STATE");
        playerController.OnSelectEntity?.Invoke(playerController.gameManager.selectedEntity);
        playerController.inputController.EntitySelectedState.Enable();
    }
    public void ExitState(PlayerController playerController)
    {
        Debug.Log("EXIT ENTITY SELECTED STATE");
        playerController.OnResetTiles?.Invoke();
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
        if (!context.performed || EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(playerController.gameManager.mousePosition);
        Tile tile = playerController.gameManager.game.map.OnHoverMapGetTile(worldPosition);

        if (tile == null)
            return;

        List<Entity> entities = tile.GetEntities() ?? new List<Entity>();

        if (entities.Count == 0)
            TryMoveSelectedEntityToTile(playerController, tile);
        else
            TrySelectEntityOnTile(playerController, entities);
    }

    public void OnDeselectEntity(InputAction.CallbackContext context, PlayerController playerController)
    {
        if (!context.performed || EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(playerController.gameManager.mousePosition);
        Tile tile = playerController.gameManager.game.map.OnHoverMapGetTile(worldPosition);

        if (tile == null)
            return;

        List<Entity> entities = tile.GetEntities() ?? new List<Entity>();
        if (entities.Count == 0)
        {
            playerController.gameManager.selectedEntity = null;
            playerController.TransitionToState(playerController.BasicState);
        }
        else
        {
            TryToAttackEntity(playerController, tile);
        }
    }

    private void TrySelectEntityOnTile(PlayerController playerController, List<Entity> entities)
    {
        foreach (Entity entity in entities)
        {
            if (entity is Figure)
            {
                playerController.gameManager.selectedEntity = entity;
                playerController.TransitionToState(playerController.EntitySelectedState);
                break;
            }
        }
    }

    private void TryMoveSelectedEntityToTile(PlayerController playerController, Tile targetTile)
    {
        Entity selectedEntity = playerController.gameManager.selectedEntity;
        MovementBehaviour movementBehaviour = selectedEntity?.GetBehaviour<MovementBehaviour>();

        if (movementBehaviour == null || !movementBehaviour.GetAvailableMoves().Contains(targetTile)) return;

        movementBehaviour.SetPath(targetTile);
        selectedEntity.AddBehaviourToWork(movementBehaviour);
        playerController.TransitionToState(playerController.BasicState);
    }

    private void TryToAttackEntity(PlayerController playerController, Tile targetTile)
    {
        Entity selectedEntity = playerController.gameManager.selectedEntity;
        AttackBehaviour attackBehaviour = selectedEntity?.GetBehaviour<AttackBehaviour>();

        if (attackBehaviour == null || !attackBehaviour.GetAttackMoves().Contains(targetTile))
            return;

        foreach (var entity in targetTile.GetEntities())
        {
            if(attackBehaviour.CanAttack(entity))
            {
                attackBehaviour.SetAttack(entity.GetBehaviour<DamageableBehaviour>());
                selectedEntity.AddBehaviourToWork(attackBehaviour);
                playerController.TransitionToState(playerController.BasicState);
            }
        }
    }
}


