using System.Collections;
using UnityEngine;

public class FireEffectController : MonoBehaviour
{
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private AudioSource fireAudio;

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.enabled = false; // stop spawning new particles immediately, existing ones finish naturally
        }

        float startVolume = fireAudio != null ? fireAudio.volume : 0f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            if (fireAudio != null)
                fireAudio.volume = Mathf.Lerp(startVolume, 0f, t);

            yield return null;
        }

        if (fireAudio != null)
            fireAudio.Stop();

        gameObject.SetActive(false);
    }
}