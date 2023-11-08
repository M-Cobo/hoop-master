using UnityEngine;
using DG.Tweening;

public class PointEffector : MonoBehaviour
{
    private SpriteRenderer outline;
    private AudioSource myAudioSource;

    private void Awake() 
    {
        outline = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        myAudioSource = GetComponent<AudioSource>();
    } 

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(other.CompareTag("Ball") || other.CompareTag("Coin") || other.CompareTag("Bomb"))
        {
            myAudioSource.Play();
            DOTween.Sequence()
                .Append(outline.DOColor(new Color32(255, 255, 255, 255), 0.2f))
                .Append(outline.DOColor(new Color32(55, 55, 55, 255), 0.2f));
        }
    }
}
