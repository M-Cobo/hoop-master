using System.Collections;
using UnityEngine;
using System;
using Doozy.Engine;
using Doozy.Engine.UI;
using EZCameraShake;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class Coin : MonoBehaviour
{
    // Serialized Fields
    [Header("General Settings")]
    [SerializeField] private IntVariable collectedCoins = null;

    [Space(2), Header("Audio SFX")]
    [SerializeField] private AudioSource[] audioSources = new AudioSource[0];

    [Space(2), Header("Animations & VFX")]
    [SerializeField] private GameObject ringCoinVFX = null;
    [SerializeField] private TweenAnimation loopAnimation = null;
    [SerializeField] private TweenAnimation scoredAnimation = null;
    [SerializeField] private TweenAnimation labelAnimation = null;

    // NonSerialized Fields
    private bool hoopTouched = false;
    private Vector2 lastPos = Vector2.zero;
    private float timer = 0f;

    private Camera _Camera = null;
    private CircleCollider2D myCollider = null;
    private Rigidbody2D myRigidbody = null;
    private SpriteRenderer mySpriteRenderer = null;
    private UIPopup _Coinsboard = null;
    private TextMeshProUGUI _CoinsText = null; 

    // Properties
    private int Coins { 
        get { return collectedCoins.Value; } 
        set 
        {
            collectedCoins.Value = value; 
            StartCoroutine(DisableMyself());
            PlaySound("Dunk", 1.0f);
            if(GameManager.Instance.vibrationState == true) { Handheld.Vibrate(); }
        } 
    }

    private void Awake() 
    {
        _Camera = Camera.main;
        myCollider = GetComponent<CircleCollider2D>();
        myRigidbody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();

        _Coinsboard = UIPopup.GetPopup("Coinsboard");
        if(_Coinsboard == null) {
            Debug.LogError(string.Format("UIPopup : Coinsboard, was not found! Please assign a valid name."), gameObject);
        }
        _CoinsText = _Coinsboard.Data.Labels[0].GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable() 
    {
        timer = 0;
        hoopTouched = false;
        myRigidbody.simulated = true;
        myRigidbody.constraints = RigidbodyConstraints2D.None;
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        loopAnimation.Play(_TransformTarget: transform, _ColorTarget: mySpriteRenderer);
    }

    private void Update() 
    {
        if(transform.position.x == lastPos.x || transform.position.y == lastPos.y)
        {
            timer += Time.deltaTime;
            if(timer >= 5.0f) {
                myRigidbody.AddForce(Vector2.one * 10, ForceMode2D.Impulse);
                timer = 0f;
            }
        }
        
        lastPos = transform.position;
    }

	private void OnCollisionEnter2D(Collision2D otherObj)
    {
        float soundVolume = 0.0f;
        float impactForce =  KineticEnergy(myRigidbody);
        if      (impactForce <= 7.5f)                       { soundVolume = 0.4f; }
        else if (impactForce > 7.5f && impactForce < 15)    { soundVolume = 0.6f; }
        else if (impactForce >= 15 && impactForce <= 22.5f) { soundVolume = 0.8f; }
        else if (impactForce > 20)                          { soundVolume = 1.0f; }

        if (otherObj.gameObject.CompareTag("Hoop")) {
            PlaySound("Hit", UnityEngine.Random.Range(1.2f, 1.4f), soundVolume);
            if(!hoopTouched) { hoopTouched = true; }
        } else {
            PlaySound("Hit", UnityEngine.Random.Range(0.8f, 1.0f), soundVolume);
        }
	}

    public static float KineticEnergy(Rigidbody2D rb)
    {
        return 0.5f * rb.mass * rb.velocity.sqrMagnitude;
    }

    private void OnTriggerExit2D(Collider2D otherObj) 
    {
		if (otherObj.CompareTag("Killzone")) { timer = 0; gameObject.SetActive(false); }
        
        if(GameManager.Instance.gameState != GameState.Playing) { return; }
        
		if (otherObj.CompareTag("Point") && myRigidbody.velocity.y < -0.25f)
        {
            Physics2D.IgnoreCollision(otherObj, myCollider);
            int points = 0;

            if (!hoopTouched) { points += 2; Coins += 2; } 
            else { points += 1; Coins += 1; }

            UIPopup popup = UIPopup.GetPopup("Points");

            Vector2 textPos = (otherObj.transform.position + Vector3.up);
            popup.transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition = textPos;

            TextMeshProUGUI text = popup.Data.Labels[0].GetComponent<TextMeshProUGUI>();
            text.SetText(string.Format("+{0}", points));
            text.color = new Color32(255, 241, 128, 255);

            popup.Show();

            popup = UIPopup.GetPopup("Words");
            popup.Data.Labels[0].GetComponent<TextMeshProUGUI>().SetText("New Coin!");
            popup.Show();
        }
    }

    IEnumerator DisableMyself()
    {
        loopAnimation.Stop();

        timer = 0;
        myRigidbody.simulated = false;
        myRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

        Instantiate(ringCoinVFX, transform.position, Quaternion.identity);

        _Coinsboard.Show();
        scoredAnimation.Play(_TransformTarget: transform, _ColorTarget: mySpriteRenderer);
        yield return new WaitForSeconds(scoredAnimation.Scale.Duration);

        Vector2 moveToV2 = _Camera.ScreenToWorldPoint(_Coinsboard.Data.Images[0].transform.position);
        Tween moveTween = transform.DOMove(moveToV2, 1.0f)
            .SetEase(Ease.OutExpo);
        
        yield return moveTween.WaitForCompletion();
        
        labelAnimation.Play(_TransformTarget: _Coinsboard.Data.Labels[0].transform);
        _CoinsText.SetText(collectedCoins.Value.ToString());
        gameObject.SetActive(false);

    }

    private void PlaySound(string name, float pitch = 1.0f, float volume = 1.0f)
    {
        AudioSource audioSource = Array.Find(audioSources, source => source.name == "AS_" + name);
        if(audioSource == null) { Debug.LogWarning("Sound: (" + name + ") Not Found!"); return; }
        
        audioSource.pitch = pitch;
        audioSource.Play();
    }
}