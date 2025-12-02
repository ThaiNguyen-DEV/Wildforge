using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    //public AudioSource earth;
    public AudioSource attack;
    public AudioSource gameover;
    //public AudioSource boost;
    //public AudioSource pause;
    //public AudioSource unpause;
    //public AudioSource boom;
    //public AudioSource hit;
    //public AudioSource shoot;
    //public AudioSource zap;
    //public AudioSource heat;
    //public AudioSource hitA;
    //public AudioSource charge;


    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlaySound(AudioSource sound)
    {
        sound.Stop();
        sound.Play();
    }

    public void PlayModifiedSound(AudioSource sound)
    {
        sound.pitch = Random.Range(0.7f, 1.3f);
        sound.Stop();
        sound.Play();
    }
}
