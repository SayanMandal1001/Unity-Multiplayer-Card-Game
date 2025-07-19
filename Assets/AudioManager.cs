using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    public static AudioManager instance;

    // Start is called before the first frame update
    void Awake()
    {
        if(instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach(Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake=false;
        }
        
    }

    private void Start()
    {
        Play("BackgroundMusic");
    }

    private void Update()
    {
        AudioSource[] audioSources = gameObject.GetComponents<AudioSource>();
        foreach(Sound s in sounds)
        {
            foreach(AudioSource audioSource in audioSources)
            {
                if (audioSource.clip == s.clip)
                {
                    audioSource.volume = s.volume;
                    break;
                }
            }
        }
    }

    public void ClickSound()
    {
        Debug.Log("ClickSound() called");
        Play("ClickSound");
    }

    public void Play(string name)
    {
        Sound s =Array.Find(sounds, sound  => sound.name == name);
        if(s == null) {
            Debug.Log("Sound with name " + name + " was not found!");
            return; 
        }
        if (s.name == "ClickSound")
        {
            s.source.PlayOneShot(s.clip); // PlayOneShot allows overlapping clicks
        }
        else
        {
            s.source.Play(); // for background music etc.
        }
    }
}
