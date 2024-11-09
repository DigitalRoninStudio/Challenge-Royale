using TMPro;
using UnityEngine;

public abstract class Visual<TEntity, TBlueprint> : MonoBehaviour
    where TEntity : Entity
    where TBlueprint : EntityBlueprint
{
    [SerializeField] private SpriteRenderer Icon;
   // [SerializeField] private TextMeshProUGUI Name;

    protected TEntity entity;
    protected TBlueprint blueprint;
    public virtual void Initialize(TEntity entity, TBlueprint blueprint)
    {
        this.entity = entity;
        this.blueprint = blueprint;
        UpdateVisuals();
    }

    public virtual void UpdateVisuals()
    {
        Icon.sprite = blueprint.Icon;
        Icon.flipX = blueprint.Fliped;
       // Name.text = blueprint.Name;
    }
}
