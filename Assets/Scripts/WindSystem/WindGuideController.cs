using System.Collections.Generic;
using UnityEngine;

public class WindGuideController : MonoBehaviour
{
    [SerializeField] private List<WindGuideStep> steps;
    [SerializeField] private WindEffect windEffectPrefab;
    [SerializeField] private Transform player;
    [SerializeField] private float idleSecondsBeforeWind = 6f;
    [SerializeField] private float cooldownAfterDismiss = 10f;

    private WindEffect activeWind;
    private WindGuideStep currentStep;
    private float idleTimer, cooldownTimer;

    private void Update()
    {
        if (activeWind != null) return;

        if (cooldownTimer > 0f) { cooldownTimer -= Time.deltaTime; return; }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleSecondsBeforeWind)
        {
            SpawnNextWind();
            idleTimer = 0f;
        }
    }

    private WindGuideStep GetNextStep()
    {
        foreach (var s in steps)
            if (!s.isFinalStep && !s.exploredFlag.runtimeValue) return s;

        foreach (var s in steps)
            if (s.isFinalStep && !s.exploredFlag.runtimeValue) return s;

        return null;
    }


    private void SpawnNextWind()
    {
        currentStep = GetNextStep();
        if (currentStep == null) return;

        activeWind = Instantiate(windEffectPrefab, player.position, Quaternion.identity);
        activeWind.Init(currentStep.targetPoint, OnWindFinished);
    }

    private void OnWindFinished()
    {
        activeWind = null;
        cooldownTimer = cooldownAfterDismiss;
    }

    // Call from a SignalListener on each destination's entry trigger
    public void MarkExplored(BoolValue flag)
    {
        flag.runtimeValue = true;
        if (currentStep != null && currentStep.exploredFlag == flag && activeWind != null)
            activeWind.Dismiss();
    }
}