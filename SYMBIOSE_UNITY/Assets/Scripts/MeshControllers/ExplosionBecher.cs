using UnityEngine;

public class ExplosionBecher : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    private bool explose = false;

    void Start()
    {
        if (animator != null)
        {
            animator.enabled = false;
        }
    }

    public void Exploser()
    {
        if (explose) return;
        explose = true;

        animator.enabled = true;
        animator.SetTrigger("Exploser");
        Debug.Log("EXPLOSION BECHER : boom !");
    }

    public void FinExplosion()
    {

    }
}