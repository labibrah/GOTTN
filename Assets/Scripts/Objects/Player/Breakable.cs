using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Breakable : MonoBehaviour
{
    private Animator animator;
    public AudioSource audioSource;
    public AudioClip smashSound;
    public PetBubble petBubble;
    public BoolValue brokenState;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (brokenState != null && brokenState.runtimeValue)
        {
            this.gameObject.SetActive(false);
        }
    }

    void Update()
    {
    }

    public virtual void Break()
    {
        if (petBubble != null)
            petBubble.playerHasSwungSword = true;

        if (brokenState != null)
        {
            brokenState.runtimeValue = true;
        }

        animator.SetBool("smash", true);
        StartCoroutine(BreakCo());
    }

    IEnumerator BreakCo()
    {
        if (audioSource != null && smashSound != null)
        {
            audioSource.PlayOneShot(smashSound);
        }
        yield return new WaitForSeconds(0.5f);
        this.gameObject.SetActive(false);
    }
}