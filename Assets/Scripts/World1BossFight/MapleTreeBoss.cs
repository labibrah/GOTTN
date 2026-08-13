using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World1BossFight
{
    [Serializable]
    public class BossQuestion
    {
        public string question;
        public string[] answers = new string[4];
        public int correctIndex;
    }

    public class MapleTreeBoss : MonoBehaviour
    {
        [Header("Health & Staging")]
        [SerializeField] private int maxHealth;
        [Range(0, 1)][SerializeField] private float mediumStageUpperPercentage;
        [Range(0, 1)][SerializeField] private float hardStageUpperPercentage;
        [Space]
        [SerializeField] private GameObject bossHeartPrefab;
        [Space]
        [SerializeField] private Color stage0Color;
        [SerializeField] private Color stage1Color;
        [SerializeField] private Color stage2Color;
        [SerializeField] private Color stage3Color;

        [Header("Extra")]
        [SerializeField] private Animator bridgeAnimator;
        [SerializeField] private GameObject enableOnFightStart;
        [SerializeField] private GameObject disableOnFightStart;
        [SerializeField] private Signal bossDefeatedSignal;

        [Header("Questions")]
        [SerializeField] private QuestionBubble questionBubble;
        [SerializeField] private BossQuestion[] bossQuestions;
        [SerializeField] private Vector3Int questionPhaseCounts = new Vector3Int(2, 2, 4);
        [SerializeField] private TextMeshProUGUI[] questions;
        [SerializeField] private Transform[] answerPositions;
        [SerializeField] private GameObject questionMapleLeafSlamPrefab;

        [Header("Attacks")]
        [SerializeField] private int attacksUntilQuestion;
        [SerializeField] private float attackCooldown;

        [Header("Rolling Log Attack")]
        [SerializeField] private GameObject rollingLogPrefab;
        [SerializeField] private Vector3Int rollingLogStageCount;
        [SerializeField] private Vector3 rollingLogStageSpeed;
        [SerializeField] private Vector3 rollingLogStageAttackSpeed;
        [SerializeField] private int maxRollingLogAttacksPerQuestionCycle = 1;
        [SerializeField] private int rollingLogCountReduction = 1;
        [SerializeField] private float rollingLogAttackSpacingMultiplier = 0.85f;
        [Space]
        [SerializeField] private Transform leftRollingLogSpawnPoint;
        [SerializeField] private Transform rightRollingLogSpawnPoint;
        [SerializeField] private float rollingLogSpawnPointHeight;

        [Header("Branch Strike Attack")]
        [SerializeField] private GameObject branchStrikePrefab;
        [SerializeField] private Vector3Int branchStrikeStageCount;
        [SerializeField] private Vector3 branchStrikeStageDelay;
        [SerializeField] private Vector3 branchStrikeStageSpeed;
        [SerializeField] private Vector3 branchStrikeStageDuration;
        [SerializeField] private Vector3 branchStrikeStageAttackSpeed;

        [Header("Maple Leaf Slam Attack")]
        [SerializeField] private GameObject mapleLeafSlamPrefab;
        [SerializeField] private Vector3Int mapleLeafSlamStageCount;
        [SerializeField] private Vector3 mapleLeafSlamStageDelay;
        [SerializeField] private Vector3 mapleLeafSlamStageAttackSpeed;

        [Header("Hedge Split Attack")]
        [SerializeField] private GameObject hedgeSplitPrefab;
        [SerializeField] private Vector3 hedgeSplitStageDelay;
        [SerializeField] private Vector3 hedgeSplitStageQuestionDuration;

        [Header("Key System")]
        [SerializeField] private Inventory inventory;

        [SerializeField] private FireEffectController[] bridgeFireEffects;
        [SerializeField] private float fireFadeOutDuration = 2f;

        // ---------------------------------------------------------------
        // Difficulty tiers, driven by how many of the 5 keys the player
        // has collected:
        //   5 keys      -> Defeatable / Easy
        //   2-4 keys    -> Hard
        //   0-1 keys    -> Undefeatable
        //
        // Per-stage (X/Y/Z) attack values below come from the parameter
        // sheet supplied by the boss fight's original creator
        // ("LinguaQuest Boss Fight Parameters"), which gives a single
        // Easy -> Hard number for each attack value. Those two numbers
        // are used as follows:
        //   - The Easy value is the Z (final in-fight phase) value for
        //     the 5-key/Easy tier.
        //   - The Hard value is the Z value for the 0-1 key/Undefeatable
        //     tier.
        //   - The 2-4 key/Hard tier sits at the midpoint between them.
        //   - Within each tier, X (early phase) and Y (mid phase) ramp
        //     up toward that tier's Z value, so the fight still gets
        //     harder as the boss loses health, on top of getting harder
        //     as fewer keys are collected.
        // Maple Leaf Slam's attack speed and Hedge Split's delay/question
        // duration are called out in the parameter sheet as being capped
        // by animation length, so none of their values go past what's
        // listed there.
        // ---------------------------------------------------------------

        [Header("0-1 Keys - Undefeatable")]
        [SerializeField] private float zeroKey_AttackCooldown = 150;
        [SerializeField] private int zeroKey_AttacksUntilQuestion = 1;

        [Header("2-4 Keys - Hard")]
        [SerializeField] private float twoKey_AttackCooldown = 4;
        [SerializeField] private int twoKey_AttacksUntilQuestion = 6;

        [Header("5 Keys - Defeatable")]
        [SerializeField] private float fiveKey_AttackCooldown = 2;
        [SerializeField] private int fiveKey_AttacksUntilQuestion = 3;

        private int _health;
        private int _attacksCount;
        private SpriteRenderer _spriteRenderer;
        private BoxCollider2D _boxCollider2D;
        private Animator _animator;
        private AudioSource _audioSource;
        private int _rollingLogAttacksThisCycle;
        private int _currentPhaseIndex;
        private readonly List<int>[] _phaseQuestionIndexes = new List<int>[3];
        private readonly List<int>[] _phaseQuestionQueue = new List<int>[3];
        private readonly int[] _lastBossQuestionIndexByPhase = { -1, -1, -1 };

        private int CountKeys()
        {
            if (inventory == null)
            {
                Debug.LogError("Boss inventory is NULL!");
                return 0;
            }

            int count = 0;
            foreach (var entry in inventory.items)
            {
                if (entry.item.itemName.Contains("Key") ||
                    entry.item.itemName.Contains("key") ||
                    entry.item.itemName == "KeyPiece")
                {
                    count += entry.quantity;
                }
            }

            return count;
        }

        private void ApplyDifficulty(int keys)
        {
            if (keys >= 5)
            {
                // DEFEATABLE / EASY (5 keys)
                attackCooldown = fiveKey_AttackCooldown;
                attacksUntilQuestion = fiveKey_AttacksUntilQuestion;

                // Rolling Log
                rollingLogStageCount = new Vector3Int(6, 8, 10);
                rollingLogStageSpeed = new Vector3(2.9f, 4.9f, 7f);
                rollingLogStageAttackSpeed = new Vector3(1.11f, 1.05f, 1f);
                maxRollingLogAttacksPerQuestionCycle = 1;
                rollingLogCountReduction = 1;
                rollingLogAttackSpacingMultiplier = 1.2f;

                // Branch Strike
                branchStrikeStageCount = new Vector3Int(9, 12, 15);
                branchStrikeStageDelay = new Vector3(1.73f, 1.62f, 1.5f);
                branchStrikeStageSpeed = new Vector3(8.7f, 11.9f, 15f);
                branchStrikeStageDuration = new Vector3(1.14f, 1.07f, 1f);
                branchStrikeStageAttackSpeed = new Vector3(0.34f, 0.32f, 0.3f);

                // Maple Leaf Slam (attack speed capped by animation length)
                mapleLeafSlamStageCount = new Vector3Int(6, 8, 10);
                mapleLeafSlamStageDelay = new Vector3(1.16f, 1.08f, 1f);
                mapleLeafSlamStageAttackSpeed = new Vector3(0.57f, 0.54f, 0.5f);

                // Hedge Split (delay/question duration capped by animation length)
                hedgeSplitStageDelay = new Vector3(8f, 7.5f, 7f);
                hedgeSplitStageQuestionDuration = new Vector3(7.9f, 7.5f, 7f);

                // More question phases = more chances to damage the boss
                questionPhaseCounts = new Vector3Int(3, 3, 4);

                Debug.Log("Boss difficulty: Defeatable (5 keys)");
            }
            else if (keys >= 2)
            {
                // HARD (2-4 keys)
                attackCooldown = twoKey_AttackCooldown;
                attacksUntilQuestion = twoKey_AttacksUntilQuestion;

                // Rolling Log
                rollingLogStageCount = new Vector3Int(19, 21, 23);
                rollingLogStageSpeed = new Vector3(14.4f, 16.4f, 18.5f);
                rollingLogStageAttackSpeed = new Vector3(0.81f, 0.75f, 0.7f);
                maxRollingLogAttacksPerQuestionCycle = 2;
                rollingLogCountReduction = 0;
                rollingLogAttackSpacingMultiplier = 0.95f;

                // Branch Strike
                branchStrikeStageCount = new Vector3Int(27, 30, 33);
                branchStrikeStageDelay = new Vector3(1.08f, 0.97f, 0.85f);
                branchStrikeStageSpeed = new Vector3(26.2f, 29.4f, 32.5f);
                branchStrikeStageDuration = new Vector3(0.74f, 0.67f, 0.6f);
                branchStrikeStageAttackSpeed = new Vector3(0.24f, 0.22f, 0.2f);

                // Maple Leaf Slam
                mapleLeafSlamStageCount = new Vector3Int(19, 21, 23);
                mapleLeafSlamStageDelay = new Vector3(0.71f, 0.63f, 0.55f);
                mapleLeafSlamStageAttackSpeed = new Vector3(0.37f, 0.34f, 0.3f);

                // Hedge Split
                hedgeSplitStageDelay = new Vector3(5.2f, 4.7f, 4.25f);
                hedgeSplitStageQuestionDuration = new Vector3(5.4f, 5f, 4.5f);

                // Fewer question phases = fewer damage chances
                questionPhaseCounts = new Vector3Int(2, 2, 3);

                Debug.Log("Boss difficulty: Hard (2-4 keys)");
            }
            else
            {
                // UNDEFEATABLE (0-1 keys)
                attackCooldown = zeroKey_AttackCooldown;
                attacksUntilQuestion = zeroKey_AttacksUntilQuestion;

                // Rolling Log
                rollingLogStageCount = new Vector3Int(31, 33, 35);
                rollingLogStageSpeed = new Vector3(25.9f, 27.9f, 30f);
                rollingLogStageAttackSpeed = new Vector3(0.51f, 0.45f, 0.4f);
                maxRollingLogAttacksPerQuestionCycle = 3;
                rollingLogCountReduction = 0;
                rollingLogAttackSpacingMultiplier = 0.7f;

                // Branch Strike
                branchStrikeStageCount = new Vector3Int(44, 47, 50);
                branchStrikeStageDelay = new Vector3(0.43f, 0.32f, 0.2f);
                branchStrikeStageSpeed = new Vector3(43.7f, 46.9f, 50f);
                branchStrikeStageDuration = new Vector3(0.34f, 0.27f, 0.2f);
                branchStrikeStageAttackSpeed = new Vector3(0.14f, 0.12f, 0.1f);

                // Maple Leaf Slam (attack speed floor set by animation length)
                mapleLeafSlamStageCount = new Vector3Int(31, 33, 35);
                mapleLeafSlamStageDelay = new Vector3(0.26f, 0.18f, 0.1f);
                mapleLeafSlamStageAttackSpeed = new Vector3(0.17f, 0.14f, 0.1f);

                // Hedge Split (delay/question duration floor set by animation length)
                hedgeSplitStageDelay = new Vector3(2.5f, 2f, 1.5f);
                hedgeSplitStageQuestionDuration = new Vector3(2.9f, 2.5f, 2f);

                // Minimal question phases = almost no damage chances
                questionPhaseCounts = new Vector3Int(1, 1, 2);

                Debug.Log("Boss difficulty: Undefeatable (0-1 keys)");
            }
        }

        private void Awake()
        {
            int keyCount = CountKeys();
            ApplyDifficulty(keyCount);

            _health = maxHealth;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.color = stage0Color;
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
            _currentPhaseIndex = 0;

            // Question phases depend on questionPhaseCounts, which ApplyDifficulty
            // just set, so this must run after ApplyDifficulty.
            InitializeQuestionPhases();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                StartCoroutine(StartBattleRoutine());
            }
        }

        private IEnumerator StartBattleRoutine()
        {
            _boxCollider2D.enabled = false;

            int keys = CountKeys();
            if (keys >= 5)
                Debug.Log("You have all 5 keys - the boss is vulnerable!");
            else if (keys >= 2)
                Debug.Log("You have " + keys + " keys - the boss will be tough!");
            else
                Debug.Log("You have no keys - the boss is undefeatable!");

            if (enableOnFightStart) enableOnFightStart.SetActive(true);
            if (disableOnFightStart) disableOnFightStart.SetActive(false);
            yield return new WaitForSeconds(1);
            bridgeAnimator.SetTrigger("Break");

            foreach (var fire in bridgeFireEffects)
            {
                if (fire != null) fire.FadeOut(fireFadeOutDuration);
            }

            StartCoroutine(ChangeStateRoutine());
            yield return new WaitForSeconds(3);
            _animator.SetTrigger("Idle");
            _audioSource.Play();
        }

        public BossQuestion GetRandomBossQuestion()
        {
            if (bossQuestions == null || bossQuestions.Length == 0) return null;

            int phaseIndex = Mathf.Clamp(_currentPhaseIndex, 0, _phaseQuestionQueue.Length - 1);
            if (_phaseQuestionQueue[phaseIndex] == null || _phaseQuestionQueue[phaseIndex].Count == 0)
            {
                RefillPhaseQuestionQueue(phaseIndex);
            }

            var phaseQuestions = _phaseQuestionQueue[phaseIndex];
            if (phaseQuestions == null || phaseQuestions.Count == 0) return null;

            int selectedIndex = 0;
            if (phaseQuestions.Count > 1 && phaseQuestions[0] == _lastBossQuestionIndexByPhase[phaseIndex])
            {
                selectedIndex = 1;
            }

            int questionIndex = phaseQuestions[selectedIndex];
            phaseQuestions.RemoveAt(selectedIndex);
            _lastBossQuestionIndexByPhase[phaseIndex] = questionIndex;
            return bossQuestions[questionIndex];
        }

        public void PerformAttack()
        {
            if (_attacksCount >= attacksUntilQuestion)
            {
                _attacksCount = 0;
                _rollingLogAttacksThisCycle = 0;
                PerformHedgeSplitAttack();
                return;
            }

            var availableAttacks = new List<int> { 0, 1, 2 };
            if (_rollingLogAttacksThisCycle >= maxRollingLogAttacksPerQuestionCycle)
            {
                availableAttacks.Remove(0);
            }

            var rand = availableAttacks[Random.Range(0, availableAttacks.Count)];
            switch (rand)
            {
                case 0:
                    PerformRollingLogAttack();
                    break;
                case 1:
                    PerformBranchStrikeAttack();
                    break;
                case 2:
                    PerformMapleLeafSlamAttack();
                    break;
            }
            _attacksCount++;
        }

        private float GetStageValue(Vector3 value)
        {
            switch (Mathf.Clamp(_currentPhaseIndex, 0, 2))
            {
                case 0:
                    return value.x;
                case 1:
                    return value.y;
                default:
                    return value.z;
            }
        }

        public void PerformRollingLogAttack()
        {
            _rollingLogAttacksThisCycle++;
            StartCoroutine(RollingLogAttackRoutine());
        }

        private IEnumerator RollingLogAttackRoutine()
        {
            var count = Mathf.Max(1, Mathf.RoundToInt(GetStageValue(rollingLogStageCount)) - rollingLogCountReduction);
            var speed = GetStageValue(rollingLogStageSpeed);
            var attackSpeed = Mathf.Max(0.1f, GetStageValue(rollingLogStageAttackSpeed) * rollingLogAttackSpacingMultiplier);
            for (var i = 0; i < count; i++)
            {
                var spawnLeft = Random.Range(0, 2) == 1;
                var spawnTransform = spawnLeft ? leftRollingLogSpawnPoint : rightRollingLogSpawnPoint;
                var spawnOffset = (int)Random.Range(-rollingLogSpawnPointHeight, rollingLogSpawnPointHeight);
                var direction = spawnLeft ? Vector3.right : Vector3.left;
                var spawnPosition = spawnTransform.position + Vector3.up * spawnOffset;

                var rollingLogGameObject = Instantiate(rollingLogPrefab, spawnPosition, Quaternion.identity);
                var rollingLog = rollingLogGameObject.GetComponent<RollingLog>();
                rollingLog.ThrowUpAndRoll(direction, speed);
                yield return new WaitForSeconds(attackSpeed);
            }
            yield return new WaitForSeconds(attackCooldown);
            PerformAttack();
        }

        public void PerformBranchStrikeAttack()
        {
            StartCoroutine(BranchStrikeAttackRoutine());
        }

        private IEnumerator BranchStrikeAttackRoutine()
        {
            var count = GetStageValue(branchStrikeStageCount);
            var delay = GetStageValue(branchStrikeStageDelay);
            var speed = GetStageValue(branchStrikeStageSpeed);
            var duration = GetStageValue(branchStrikeStageDuration);
            var attackSpeed = GetStageValue(branchStrikeStageAttackSpeed);
            for (var i = 0; i < count; i++)
            {
                var branchStrikeGameObject = Instantiate(branchStrikePrefab);
                var branchStrike = branchStrikeGameObject.GetComponent<BranchStrike>();
                branchStrike.Strike(delay, speed, duration);
                yield return new WaitForSeconds(attackSpeed);
            }
            yield return new WaitForSeconds(delay + duration + attackCooldown);
            PerformAttack();
        }

        public void PerformMapleLeafSlamAttack()
        {
            StartCoroutine(MapleLeafSlamAttackRoutine());
        }

        private IEnumerator MapleLeafSlamAttackRoutine()
        {
            var count = GetStageValue(mapleLeafSlamStageCount);
            var delay = GetStageValue(mapleLeafSlamStageDelay);
            var attackSpeed = GetStageValue(mapleLeafSlamStageAttackSpeed);
            for (var i = 0; i < count; i++)
            {
                var mapleLeafSlamGameObject = Instantiate(mapleLeafSlamPrefab);
                var mapleLeafSlam = mapleLeafSlamGameObject.GetComponent<MapleLeafSlam>();
                mapleLeafSlam.Slam(delay);
                yield return new WaitForSeconds(attackSpeed);
            }
            yield return new WaitForSeconds(delay + attackCooldown);
            PerformAttack();
        }

        private void PerformHedgeSplitAttack()
        {
            StartCoroutine(HedgeSplitSlamAttackRoutine());
        }

        private IEnumerator HedgeSplitSlamAttackRoutine()
        {
            var delay = GetStageValue(hedgeSplitStageDelay);
            var duration = GetStageValue(hedgeSplitStageQuestionDuration);

            var hedgeSplitGameObject = Instantiate(hedgeSplitPrefab);
            var hedgeSplit = hedgeSplitGameObject.GetComponent<HedgeSplit>();

            hedgeSplit.Split(delay, duration);

            var question = GetRandomBossQuestion();
            if (question == null)
            {
                yield return new WaitForSeconds(delay + duration + attackCooldown);
                PerformAttack();
                yield break;
            }
            questionBubble.ShowMessage(question.question, delay);

            var setIndexes = new List<int>();
            var correctIndex = 0;
            var answerCount = question.answers.Length;
            for (var i = 0; i < answerCount; i++)
            {
                int index = Random.Range(0, answerCount);
                while (setIndexes.Contains(index)) index = (index + 1 + answerCount) % answerCount;
                setIndexes.Add(index);
                questions[i].text = question.answers[index];
                if (index == question.correctIndex) correctIndex = i;
            }

            yield return new WaitForSeconds(delay);

            questions[0].text = questions[1].text = questions[2].text = questions[3].text = string.Empty;

            var currentHealth = _health;
            StartCoroutine(SpawnHeartRoutine(correctIndex, duration));
            SpawnWrongAnswerSlams(correctIndex);

            yield return new WaitForSeconds(duration + attackCooldown);

            if (currentHealth == _health)
            {
                PerformAttack();
                yield break;
            }

            StartCoroutine(_health <= 0 ? DieRoutine() : ChangeStateRoutine());
        }

        private IEnumerator SpawnHeartRoutine(int correctIndex, float duration)
        {
            var heart = Instantiate(
                bossHeartPrefab,
                answerPositions[correctIndex].position,
                Quaternion.identity
            );

            var bossHeart = heart.GetComponent<BossHeart>();
            bossHeart.Damaged += BossHeartOnDamaged;

            yield return new WaitForSeconds(duration);
            bossHeart.Damaged -= BossHeartOnDamaged;
            Destroy(heart);
        }

        private void BossHeartOnDamaged(BossHeart bossHeart)
        {
            bossHeart.Damaged -= BossHeartOnDamaged;
            _health--;
            _currentPhaseIndex = Mathf.Clamp(maxHealth - _health, 0, 2);
        }

        private void SpawnWrongAnswerSlams(int correctIndex)
        {
            StartCoroutine(SlamWrongAnswersRoutine(correctIndex));
        }

        private IEnumerator SlamWrongAnswersRoutine(int correctIndex)
        {
            yield return new WaitForSeconds(1f);

            for (int i = 0; i < answerPositions.Length; i++)
            {
                if (i == correctIndex) continue;

                var slam = Instantiate(questionMapleLeafSlamPrefab);

                var slamComp = slam.GetComponent<MapleLeafSlam>();
                var position = answerPositions[i].position;
                slamComp.transform.position = position;
                slamComp.SlamAtPosition(0.5f);
                yield return new WaitForSeconds(0.4f);
            }
        }

        private IEnumerator ChangeStateRoutine()
        {
            var stage = Mathf.Clamp(_currentPhaseIndex, 0, 2);
            var colorA = Color.white;
            var colorB = Color.white;
            switch (stage)
            {
                case 0:
                    colorA = stage0Color;
                    colorB = stage1Color;
                    break;
                case 1:
                    colorA = stage1Color;
                    colorB = stage2Color;
                    break;
                case 2:
                    colorA = stage2Color;
                    colorB = stage3Color;
                    break;
            }

            float timer = 0;
            while (timer < 3)
            {
                timer += Time.deltaTime;
                _spriteRenderer.color = Color.Lerp(colorA, colorB, timer / 3);
                yield return null;
            }

            PerformAttack();
        }

        private void InitializeQuestionPhases()
        {
            for (int phaseIndex = 0; phaseIndex < 3; phaseIndex++)
            {
                _phaseQuestionIndexes[phaseIndex] = new List<int>();
                _phaseQuestionQueue[phaseIndex] = new List<int>();
            }

            if (bossQuestions == null || bossQuestions.Length == 0) return;

            int[] phaseCounts =
            {
                Mathf.Max(0, questionPhaseCounts.x),
                Mathf.Max(0, questionPhaseCounts.y),
                Mathf.Max(0, questionPhaseCounts.z)
            };

            int bossQuestionIndex = 0;
            for (int phaseIndex = 0; phaseIndex < phaseCounts.Length && bossQuestionIndex < bossQuestions.Length; phaseIndex++)
            {
                for (int count = 0; count < phaseCounts[phaseIndex] && bossQuestionIndex < bossQuestions.Length; count++)
                {
                    _phaseQuestionIndexes[phaseIndex].Add(bossQuestionIndex);
                    bossQuestionIndex++;
                }
            }

            while (bossQuestionIndex < bossQuestions.Length)
            {
                _phaseQuestionIndexes[2].Add(bossQuestionIndex);
                bossQuestionIndex++;
            }

            for (int phaseIndex = 0; phaseIndex < 3; phaseIndex++)
            {
                if (_phaseQuestionIndexes[phaseIndex].Count == 0)
                {
                    _phaseQuestionIndexes[phaseIndex].AddRange(_phaseQuestionIndexes[Mathf.Max(0, phaseIndex - 1)]);
                }

                RefillPhaseQuestionQueue(phaseIndex);
            }
        }

        private void RefillPhaseQuestionQueue(int phaseIndex)
        {
            if (_phaseQuestionIndexes[phaseIndex] == null) return;

            _phaseQuestionQueue[phaseIndex].Clear();
            _phaseQuestionQueue[phaseIndex].AddRange(_phaseQuestionIndexes[phaseIndex]);

            for (int i = _phaseQuestionQueue[phaseIndex].Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                int temp = _phaseQuestionQueue[phaseIndex][i];
                _phaseQuestionQueue[phaseIndex][i] = _phaseQuestionQueue[phaseIndex][randomIndex];
                _phaseQuestionQueue[phaseIndex][randomIndex] = temp;
            }
        }

        private IEnumerator DieRoutine()
        {
            _audioSource.Stop();
            _animator.SetTrigger("Die");
            yield return new WaitForSeconds(3f);
            bridgeAnimator.SetTrigger("Show");
            if (enableOnFightStart) enableOnFightStart.SetActive(false);
            if (disableOnFightStart) disableOnFightStart.SetActive(true);
            bossDefeatedSignal?.Raise();
        }
    }
}