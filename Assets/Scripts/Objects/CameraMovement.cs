using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float smoothing;
    public Animator cameraAnimator;
    private bool hasSnapped = false; 

    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
        cameraAnimator = GetComponent<Animator>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (!hasSnapped)
        {
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
            hasSnapped = true;
            return;
        }

        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        float distance = Vector3.Distance(transform.position, targetPosition);
        float dynamicSmoothing = smoothing * distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, dynamicSmoothing * Time.deltaTime);
    }

    public void DoScreenKick()
    {
        StartCoroutine(ScreenKick());
    }

    public IEnumerator ScreenKick()
    {
        cameraAnimator.SetBool("KickActive", true);
        yield return new WaitForSeconds(0.1f);
        cameraAnimator.SetBool("KickActive", false);
    }

    public void PayAttentionTo(GameObject thing)
    {
        StartCoroutine(PayAttentionToThings(thing, 2f));
    }

    private IEnumerator PayAttentionToThings(GameObject thing, float duration)
    {
        target = thing.transform;
        yield return new WaitForSeconds(duration);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }

    public void PayAttentionTo(GameObject thing, float duration)
    {
        StartCoroutine(PayAttentionToThings(thing, duration));
    }
}