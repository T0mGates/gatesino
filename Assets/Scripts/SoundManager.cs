using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public string[] names;
    public AudioSource[] sounds;

    private Dictionary<string, AudioSource> audioDict = new Dictionary<string, AudioSource>();
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < names.Length; i++)
        {
            audioDict.Add(names[i], sounds[i]);
        }
    }

    public void PlaySound(string name)
    {
        audioDict[name].Play();
    }

    public void StopSound(string name)
    {
        audioDict[name].Stop();
    }
}
