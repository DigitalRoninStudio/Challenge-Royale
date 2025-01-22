using UnityEngine;

public class SwordsmanSpecialVisual : GenericBehaviourVisual<SwordsmanSpecial>
{
    protected override void InitializeVisual()
    {
        Animator animator = ((Figure)behaviour.Owner).GameObject.GetComponent<FigureVisual>().animator;

        if (animator == null) return;

        if(behaviour.IsToogle)
            animator?.SetBool("Def", true);

        behaviour.OnToggle += (toggle) =>
        {
            if (toggle)
                animator?.SetBool("Def", true);
            else
                animator?.SetBool("Def", false);
        };
    }
}
