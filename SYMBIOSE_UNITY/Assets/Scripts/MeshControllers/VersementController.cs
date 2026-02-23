using UnityEngine;

public class VersementController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    private bool enCours = false;

    public void Verser()
    {
        if (enCours) return;

        enCours = true;
        animator.SetTrigger("Verser");

        Debug.Log("VERSEMENT : animation lancee");
    }

    // appele par Animation Event a la fin de l'anim
    public void FinVersement()
    {
        enCours = false;
        Debug.Log("VERSEMENT : animation terminee");
    }
}