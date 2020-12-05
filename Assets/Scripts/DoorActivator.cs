﻿using System;
using UnityEngine;

public class DoorActivator : MonoBehaviour
{
    [SerializeField] private MultiSound doorSound = default;
    private AudioManager _audioManager;
    private MultiSoundInstance _doorSound;
    
    public bool StartOpen;
    public bool AnimateOnStart;
    public float animationTime;
    public float cameraShakeAmount;
    bool isOpen;
    NewSlidingDoor[] slidingDoors;

    private void Awake()
    {
        slidingDoors = GetComponentsInChildren<NewSlidingDoor>();
        isOpen = StartOpen;
        foreach (var door in slidingDoors)
        {
            door.SetDoor(isOpen);
            door.animationTime = animationTime;
            door.AnimateOnStart = AnimateOnStart;
        }

        foreach (var door in slidingDoors)
        {
            door.FinishMoving += OnFinishedMoving;
        }
    }

    private void OnFinishedMoving(object sender, EventArgs args)
    {
        _audioManager.StopSound(_doorSound, gameObject);
        CameraFX.instance.SetFrozen(false, 0);
    }
    
    private void OnDisable()
    {
        _audioManager.Dispose(gameObject);
    }

    private void Start()
    {
        _audioManager = AudioManager.Instance();
        _doorSound = doorSound.GenerateInstance();
        
        if (AnimateOnStart)
        {
            _audioManager.PlaySound(_doorSound, gameObject);
            CameraFX.instance.AddTrauma(cameraShakeAmount);
            CameraFX.instance.SetFrozen(true, animationTime);
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        SetDoor();
    }

    public void CloseDoor()
    {
        isOpen = false;
        SetDoor();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
        SetDoor();
    }

    void SetDoor()
    {
        if (_doorSound.IsInactive) _audioManager.PlaySound(_doorSound, gameObject);
        
        if (cameraShakeAmount != 0 && !CameraFX.instance.IsFrozen)
            CameraFX.instance.AddTrauma(cameraShakeAmount);

        CameraFX.instance.SetFrozen(true, animationTime);

        foreach (var door in slidingDoors)
            door.SetDoor(isOpen);
    }

    private void OnValidate()
    {
        var tempDoors = GetComponentsInChildren<NewSlidingDoor>();

        foreach (var door in tempDoors)
            if (StartOpen)
                door.transform.localPosition = AnimateOnStart ? door.closedPosition : door.openPosition;
            else
                door.transform.localPosition = AnimateOnStart ? door.openPosition : door.closedPosition;
    }
}
