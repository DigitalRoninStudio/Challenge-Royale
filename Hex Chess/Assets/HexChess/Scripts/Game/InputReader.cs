using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

public class InputReader : MonoBehaviour, PlayerInGameController.IMouseInputActions
{
    public PlayerInGameController inputController;

    public Action<Vector2> OnMousePosition;
    public Action OnLeftClick;
    public Action OnRightClick;


    private void Awake()
    {
        inputController = new PlayerInGameController();
        inputController.MouseInput.SetCallbacks(this);
        inputController.MouseInput.Enable();
    }
    private void OnEnable()
    {
        inputController.Enable();
    }

    private void OnDisable()
    {
        inputController.Disable();
    }

    void PlayerInGameController.IMouseInputActions.OnMouseMove(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnMousePosition?.Invoke(context.ReadValue<Vector2>());
    }

    void PlayerInGameController.IMouseInputActions.OnLeftClick(InputAction.CallbackContext context)
    {
        if (!context.performed || EventSystem.current.IsPointerOverGameObject())
            return;

        OnLeftClick?.Invoke();
    }

    void PlayerInGameController.IMouseInputActions.OnRightClick(InputAction.CallbackContext context)
    {
        if (!context.performed || EventSystem.current.IsPointerOverGameObject())
            return;

        OnRightClick?.Invoke();
    }
}

public class PlayerController
{
    public InputReader inputReader { get; private set; }
    public Game game { get; private set; }

    public IPlayerState IdleState { get; private set; }
    public IPlayerState EntitySelectedState { get; private set; }

    private IPlayerState currentState;

    public Entity selectedEntity;

    public PlayerController(InputReader inputReader, Game game)
    {
        this.inputReader = inputReader;
        this.game = game;

        IdleState = new IdleState();
        EntitySelectedState = new EntitySelectedState();

        currentState = IdleState;
        currentState.EnterState(this);
    }
    public void TransitionToState(IPlayerState newState)
    {
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }
}

public interface IPlayerState
{
    void EnterState(PlayerController playerController);
    void ExitState(PlayerController playerController);
}
public class IdleState : IPlayerState
{
    PlayerController playerController;
    Vector2 MousePosition = Vector2.zero;
    public void EnterState(PlayerController playerController)
    {
        //Debug.Log("ENTER IDLE STATE");
        this.playerController = playerController;
        playerController.inputReader.OnLeftClick += OnLeftClick;
        playerController.inputReader.OnMousePosition += OnMousePosition;
    }

    private void OnMousePosition(Vector2 mousePosition)
    {
        MousePosition = mousePosition;
    }

    private void OnLeftClick()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(MousePosition);
        Tile tile = playerController.game.map.OnHoverMapGetTile(worldPosition);

        if (tile == null)
            return;

        List<Entity> entities = tile.GetEntities() ?? new List<Entity>();

        foreach (Entity entity in entities)
            if (entity is Figure)
            {
                playerController.selectedEntity = entity;
                playerController.TransitionToState(playerController.EntitySelectedState);
                break;
            }
    }

    public void ExitState(PlayerController playerController)
    {
        //Debug.Log("EXIT IDLE STATE");
        playerController.inputReader.OnLeftClick -= OnLeftClick;
        playerController.inputReader.OnMousePosition -= OnMousePosition;
        LocalPlayer.Instance.ClearTiles();
    }
}

public class EntitySelectedState : IPlayerState
{
    PlayerController playerController;
    Vector2 MousePosition = Vector2.zero;
    public void EnterState(PlayerController playerController)
    {
        //Debug.Log("ENTER ENTITY SELECTED STATE");
        this.playerController = playerController;
        playerController.inputReader.OnLeftClick += OnLeftClick;
        playerController.inputReader.OnRightClick += OnRightClick;
        playerController.inputReader.OnMousePosition += OnMousePosition;
        LocalPlayer.Instance.PaintTilesOnSelectEntity(playerController.selectedEntity);
    }

    private void OnMousePosition(Vector2 mousePosition)
    {
        MousePosition = mousePosition;
    }

    private void OnLeftClick()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(MousePosition);
        Tile tile = playerController.game.map.OnHoverMapGetTile(worldPosition);

        if (tile == null)
            return;

        List<Entity> entities = tile.GetEntities() ?? new List<Entity>();

        if (entities.Count == 0)
            TryMoveSelectedEntityToTile(tile);
        else
            TrySelectEntityOnTile(entities);
    }

    private void OnRightClick()
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(MousePosition);
        Tile tile = playerController.game.map.OnHoverMapGetTile(worldPosition);

        if (tile == null)
            return;

        List<Entity> entities = tile.GetEntities() ?? new List<Entity>();
        if (entities.Count == 0)
        {
            playerController.selectedEntity = null;
            playerController.TransitionToState(playerController.IdleState);
        }
        else
            TryToAttackEntity(tile);
    }

    public void ExitState(PlayerController playerController)
    {
       // Debug.Log("EXIT ENTITY SELECTED STATE");
        playerController.inputReader.OnLeftClick -= OnLeftClick;
        playerController.inputReader.OnRightClick -= OnRightClick;
        playerController.inputReader.OnMousePosition -= OnMousePosition;
        LocalPlayer.Instance.ClearTiles();
    }

    private void TryMoveSelectedEntityToTile(Tile targetTile)
    {
        if (!LocalPlayer.Instance.IsLocalClientOwner(playerController.selectedEntity) || !LocalPlayer.Instance.CanLocalPlayerDoAction()) return;

        MovementBehaviour movementBehaviour = playerController.selectedEntity?.GetBehaviour<MovementBehaviour>();

        if (movementBehaviour == null || !movementBehaviour.CanMove(targetTile)) return;

        NetMovement request = new NetMovement()
        {
            MatchId = GameManager.Instance.GetFirstMatch().GUID,
            EntityId = playerController.selectedEntity.guid,
            MovementBehaviourId = movementBehaviour.guid,
            TileCoordinate = targetTile.coordinate
        };

        Sender.ClientSendData(request, Pipeline.Reliable);
        playerController.TransitionToState(playerController.IdleState); // ?
    }

    private void TrySelectEntityOnTile(List<Entity> entities)
    {
        foreach (Entity entity in entities)
        {
            if (entity is Figure)
            {
                playerController.selectedEntity = entity;
                LocalPlayer.Instance.ClearTiles();
                LocalPlayer.Instance.PaintTilesOnSelectEntity(entity);
                break;
            }
        }
    }

    private void TryToAttackEntity(Tile targetTile)
    {
        if (!LocalPlayer.Instance.IsLocalClientOwner(playerController.selectedEntity) || !LocalPlayer.Instance.CanLocalPlayerDoAction()) return;

        AttackBehaviour attackBehaviour = playerController.selectedEntity?.GetBehaviour<AttackBehaviour>();

        if (attackBehaviour == null)
            return;

        foreach (var entity in targetTile.GetEntities())
        {
            if (attackBehaviour.CanAttack(entity))
            {
                NetAttack request = new NetAttack()
                {
                    MatchId = GameManager.Instance.GetFirstMatch().GUID,
                    AttackerEntityId = playerController.selectedEntity.guid,
                    AttackBehaviourId = attackBehaviour.guid,
                    DamagableEntityId = entity.guid,
                    DamagableBehaviourId = entity.GetBehaviour<DamageableBehaviour>().guid
                };

                Sender.ClientSendData(request, Pipeline.Reliable);
                playerController.TransitionToState(playerController.IdleState); // ?
            }
        }
    }
}



