using System;
using System.Collections;
using UnityEngine;

namespace World1BossFight
{
    public class HedgeSplit : MonoBehaviour
    {
        private static readonly int SplitHash = Animator.StringToHash("Split");
        private static readonly int HideHash = Animator.StringToHash("Hide");

        [SerializeField] private float damageAmount = 1f;
        [SerializeField] private float dissolveDelay;
        // removed bossDamagedSignal — not needed here

        private Animator _animator;
        private AudioSource _audioSource;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }

        public void Split(float delay, float duration)
        {
            //PlatformManager.Instance.ReserveHedgeSplitPositions();
            StartCoroutine(SplitRoutine(delay, duration));
        }

        private IEnumerator SplitRoutine(float delay, float duration)
        {
            yield return new WaitForSeconds(delay);
            _animator.SetTrigger(SplitHash);
            _audioSource.Play();
            yield return new WaitForSeconds(duration);
            _animator.SetTrigger(HideHash);
            yield return new WaitForSeconds(dissolveDelay);
            
            Destroy(gameObject);
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerExploring player = other.GetComponent<PlayerExploring>();
                if (player != null)
                    player.TakeDamage(damageAmount);

                StopAllCoroutines();
                StartCoroutine(DestroyRoutine());
            }
        }

        private IEnumerator DestroyRoutine()
        {
            _animator.SetTrigger(HideHash);
            yield return new WaitForSeconds(dissolveDelay);
            Destroy(gameObject);
        }
    }
}
