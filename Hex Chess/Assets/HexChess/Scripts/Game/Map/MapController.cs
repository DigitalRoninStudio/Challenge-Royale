using UnityEngine;

public class MapController : MonoBehaviour
{
    private GameManager gameManager => GameManager.Instance;
    private Map map;
    public void Initialize(Map map)
    {
        this.map = map;
        gameManager.playerController.OnResetTiles += OnResetTiles;
        gameManager.playerController.OnSelectEntity += OnSelectEntity;
    }

    private void OnSelectEntity(Entity entity)
    {

        MovementBehaviour movementBehaviour = entity.GetBehaviour<MovementBehaviour>();
        AttackBehaviour attackBehaviour = entity.GetBehaviour<AttackBehaviour>();
        Tile tile = map.GetTile(entity);

        if (tile == null) return;

        if(movementBehaviour != null)
            foreach (var availableTile in movementBehaviour.GetAvailableMoves())
                availableTile.SetColor(Color.cyan);

        if(attackBehaviour != null)
            foreach (var availableTile in attackBehaviour.GetAttackMoves())
                availableTile.SetColor(Color.red);
    }

    private void OnResetTiles()
    {
        foreach (var tile in map.Tiles) { tile.RefreshColor(); }
    }

    private void OnDisable()
    {
        gameManager.playerController.OnResetTiles -= OnResetTiles;
        gameManager.playerController.OnSelectEntity -= OnSelectEntity;

    }

}
