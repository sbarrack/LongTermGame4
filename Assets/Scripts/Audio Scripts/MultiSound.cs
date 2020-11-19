﻿using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "New MultiSound", menuName = "Audio Custom/MultiSound", order = 2)]
public class MultiSound : Sound
{
    [Header("MultiSound Settings")] 
    
    [Tooltip("The clip that is played before the looping clips.")]
    public AudioClip introClip = default;

    [Tooltip("The clip that is played when the looping clips are ended.")]
    public AudioClip outroClip = default;

    [Tooltip("How long this MultiSound will take to crossfade")]
    public float crossfadeDuration = 1;

    public new MultiSoundInstance GenerateInstance()
    {
        return new MultiSoundInstance(this);
    }
}

public class MultiSoundInstance : SoundInstance
{
    private MultiSoundState _state = MultiSoundState.Inactive;
    public MultiSoundState State => _state;

    private AudioClip _introClip, _outroClip;
    private float _crossfadeDuration;
    public double IntroDuration => (double) _introClip.samples / _introClip.frequency;
    public double OutroDuration => (double) _outroClip.samples / _outroClip.frequency;
    public MultiSoundInstance(MultiSound sound) : base(sound)
    {
        _introClip = sound.introClip;
        _outroClip = sound.outroClip;
        _crossfadeDuration = sound.crossfadeDuration;
    }
    
    public override IEnumerator PlayOnSource(AudioSource mainSource, AudioSource schedulingSource)
    {
        IsInactive = false;

        yield return PlayIntro(mainSource, schedulingSource);
        yield return PlayLooping(schedulingSource);
        yield return PlayOutro(mainSource, schedulingSource);
    }
    
    // This stage cannot be canceled programatically: it will always play to completion
    private IEnumerator PlayIntro(AudioSource intro, AudioSource looping)
    {
        _state = MultiSoundState.Intro;
    
        // Prepare both AudioSources for playback.PlayOnSource(intro);
        _settings.ApplyToSource(intro, _introClip);
        intro.loop = false;
        
        _settings.ApplyToSource(looping);
        
        // Play the intro, and crossfade the looping clip in right before the intro finishes
        intro.PlayScheduled(AudioSettings.dspTime + 0.1);
        yield return new WaitForSecondsRealtime((float) (0.1 + IntroDuration - _crossfadeDuration));
        yield return Crossfade(looping, intro);
    }

    // This stage can be canceled programatically by setting IsInactive to true
    private IEnumerator PlayLooping(AudioSource looping)
    {
        _state = MultiSoundState.Looping;

        // Standard modulation code for the looping source, taken from base Sound class
        var startTime = Time.time;
        
        while (!IsInactive) {
            var elapsedTime = Time.time - startTime;
            
            // Iterate through the modulated values, and call their modulate method
            foreach (var val in _settings.ModulatedValues)
            {
                SoundValue value = val.soundValue;
                float target = val.modulator.Modulate(val.value, elapsedTime);
                
                SetValue(value, target);
            }

            _settings.ApplyToSource(looping);
            IsInactive |= !looping.isPlaying;
            yield return new WaitForEndOfFrame();
        }
    }

    // This stage cannot be canceled programatically: it will always play to completion
    private IEnumerator PlayOutro(AudioSource outro, AudioSource looping)
    {
        // Prepare both AudioSources for playback
        _settings.ApplyToSource(outro, _outroClip);
        outro.loop = false;

        yield return Crossfade(outro, looping);

        // Wait for the outro to finish playing
        _state = MultiSoundState.Outro;
        yield return new WaitWhile(() => outro.isPlaying);
        
        // Make sure the outro has finished and reset state
        _state = MultiSoundState.Inactive;
        IsInactive = true;
    }

    private IEnumerator Crossfade(AudioSource fadeIn, AudioSource fadeOut)
    {
        // Prepare the AudioSources for the crossfade after a short delay.
        // (The delay is added to ensure that everything is happening at the same time)
        float startTime = Time.time;
        fadeIn.volume = 0;
        fadeIn.PlayScheduled(AudioSettings.dspTime + 0.1);
        fadeOut.SetScheduledEndTime(AudioSettings.dspTime + 0.1 + _crossfadeDuration);
        yield return new WaitForSeconds(0.1f);
        
        // Over the crossfadeDuration, fadeIn gets louder as fadeOut gets quieter
        while (Time.time - startTime <= _crossfadeDuration)
        {
            float percentComplete = (Time.time - startTime) / _crossfadeDuration;
            fadeIn.volume = GetValue(SoundValue.Volume) * percentComplete;
            fadeOut.volume = GetValue(SoundValue.Volume) * (1 - percentComplete);
            yield return new WaitForEndOfFrame();
        }
    }

    public override string ToString()
    {
        return base.ToString() + $"\nMultiSound State: {_state}";
    }
}

public enum MultiSoundState
{
    Intro,
    Looping,
    Outro,
    Inactive
}