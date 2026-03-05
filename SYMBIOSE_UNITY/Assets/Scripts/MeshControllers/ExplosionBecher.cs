using UnityEngine;

public class ExplosionBecher : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    private bool explose = false;

    public void Exploser()
    {
        if (explose) return;

        explose = true;

        // activer et lancer animation
        gameObject.SetActive(true);
        animator.SetTrigger("Exploser");

        Debug.Log("EXPLOSION BECHER : boom !");
    }

    // appele par Animation Event a la fin
    public void FinExplosion()
    {
        
    }
}