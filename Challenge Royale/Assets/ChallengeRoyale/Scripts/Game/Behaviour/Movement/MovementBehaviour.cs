using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class MovementBehaviour : Behaviour
{
    public float Speed { get; set; }
    public int Range { get; set; }

    protected Queue<Tile> path;
    protected Tile currentTile;

    private Tile nextTile;
    private Vector3 direction;
    private Queue<float> steps;
    public MovementBehaviour() : base()
    {
        path = new Queue<Tile>();
        steps = new Queue<float>();
    }
    public MovementBehaviour(Entity entity, float speed, int range) : base(entity) 
    { 
        Speed = speed;
        Range = range;
        path = new Queue<Tile>();
        steps = new Queue<float>();
    }
    public override void Execute()
    {
        if (nextTile == null && path.Count == 0)
        {
            currentTile = null;
            Exit();
            return;
        }

        if (nextTile == null && path.Count > 0)
        {
            nextTile = path.Dequeue();

            direction = (nextTile.GetPosition() - entity.GameObject.transform.position).normalized;
            float movePerFrame = Speed * Time.fixedDeltaTime;
            float distance = Vector3.Distance(nextTile.GetPosition(), entity.GameObject.transform.position);
            int numberOfSteps = Mathf.FloorToInt(distance / movePerFrame);

            for (int i = 0; i < numberOfSteps; i++)
                steps.Enqueue(movePerFrame);
        }

        if (steps.Count == 0)
        {
            currentTile.RemoveEntity(entity);
            currentTile = nextTile;
            currentTile.AddEntity(entity);
            nextTile = null; 
        }
        else
            entity.GameObject.transform.position += direction * steps.Dequeue();

    }

    public abstract List<Tile> GetAvailableMoves(Tile tile, Map map);

    public virtual void SetPath(Tile start, Tile end)
    {
        time = Time.time;
        currentTile = start;
    }
    public override string Serialize()
    {
        MovementBehaviourData movementData = new MovementBehaviourData
        {
            StartCoord = path.Peek().coordinate,
            EndCoord = path.Last().coordinate,
        };

        return CustomConverters.Serialize(movementData);
    }
    public override void Deserialize(string data)
    {
        MovementBehaviourData movementData = CustomConverters.Deserialize<MovementBehaviourData>(data);

        Tile start = entity.Map.GetTile(movementData.StartCoord);
        Tile end = entity.Map.GetTile(movementData.EndCoord);
        SetPath(start, end);
    }

    public class MovementBehaviourData : BehaviourData
    {
        public Vector2Int StartCoord { get; set; }
        public Vector2Int EndCoord { get; set; }
    }
}



