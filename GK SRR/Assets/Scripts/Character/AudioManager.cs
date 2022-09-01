using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] public AudioSource musicSource;
    [SerializeField] public AudioSource sfxSource;
    [SerializeField] AudioClip clip1;
    [SerializeField] AudioClip clip2;
    [SerializeField] AudioClip clip3;
    [SerializeField] AudioClip clip4;
    [SerializeField] AudioClip clip5;
    [SerializeField] AudioClip SFX1;
    [SerializeField] AudioClip SFX2;
    [SerializeField] AudioClip SFX3;
    [SerializeField] AudioClip SFX4;
    public int currentMusic;
    public static AudioManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public void PlayMusic(int SceneIndex, bool loop = true)
    {
        musicSource.Stop();
        AudioClip clip;
        switch(SceneIndex)
        {
            case 6: //Battle
                musicSource.clip = clip5;
                break;
            case 1: //Home Town
                musicSource.clip = clip1; 
                break;
            case 2: //Route 1
                musicSource.clip = clip2;
                break;
            case 3: //City Town
                musicSource.clip = clip3;
                break;
            case 4: //Route 2
                musicSource.clip = clip2;
                break;
            case 5: //Route 3
                musicSource.clip = clip4;
                break;   
            default:
                musicSource.clip = clip2;
                break;
        }
        musicSource.loop = loop;
        currentMusic = SceneIndex;
        musicSource.Play();   
    }

    public void PlaySFX(int soundType, bool loop = false)
    {
        AudioClip clip;
        switch (soundType)
        {
            case 1: //Hit
                sfxSource.clip = SFX1;
                break;
            case 2: //Attack
                sfxSource.clip = SFX2;
                break;
            case 3: //Grass
                sfxSource.clip = SFX3;
                break;
            case 4: //Rock
                sfxSource.clip = SFX4;
                break;        
            default:
                sfxSource.clip = SFX1;
                break;
        }
        sfxSource.loop = loop;
        if(!sfxSource.isPlaying)
            sfxSource.Play();
    }
}

public enum MusicClip
{
    HomeTown = 1, Route1, CityTown, Route2, Route3, Battle
};

public enum SFXClip
{
    Hit = 1, Route1, Attack, Grass, Rock
};