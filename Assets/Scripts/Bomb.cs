using System.Collections;
using UnityEngine;
using System;
using Doozy.Engine;
using Doozy.Engine.UI;
using EZCameraShake;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class Bomb : MonoBehaviour
{
    // Serialized Fields
    [Header("General Settings")]
    [SerializeField] private IntVariable currentPoints = null;
    [SerializeField] private IntVariable perfectDunks = null;

    [Space(2), Header("Audio FX")]
    [SerializeField] private AudioSource[] audioSources = new AudioSource[0];

    [Space(2), Header("VFX")]
    [SerializeField] private ParticleSystem tinyExplosion = null;
    [SerializeField] private ParticleSystem auraExplosion = null;

    // NonSerialized Fields
    private bool hoopTouched = false;
    private Vector2 lastPos = Vector2.zero;
    private Camera _Camera = null;
    private RippleEffect rippleEffect = null;
    private float timer = 0f;

    private CircleCollider2D myCollider = null;
    private Rigidbody2D myRigidbody = null;
    private SpriteRenderer mySpriteRenderer = null;

    private int PerfectDunks { 
        get { return perfectDunks.Value; } 
        set { if(value < 4) { perfectDunks.Value = value; }
        } 
    }

    private void Awake() 
    {
        timer = 0;
        _Camera = Camera.main;
        rippleEffect = _Camera.GetComponent<RippleEffect>();
        myCollider = GetComponent<CircleCollider2D>();
        myRigidbody = GetComponent<Rigidbody2D>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable() 
    {
        hoopTouched = false;
        myRigidbody.simulated = true;
        mySpriteRenderer.enabled = true;
        myRigidbody.AddTorque(UnityEngine.Random.Range(-1.00f, 1.00f), ForceMode2D.Impulse);
    }

    private void Update() 
    {
        if(transform.position.x == lastPos.x || transform.position.y == lastPos.y)
        {
            timer += Time.fixedDeltaTime;
            if(timer >= 5.0f) {
                myRigidbody.AddForce(Vector2.one * 10, ForceMode2D.Impulse);
                timer = 0f;
            }
        }
        
        lastPos = transform.position;
    }

	private void OnCollisionEnter2D(Collision2D otherObj)
    {
        myRigidbody.AddTorque(UnityEngine.Random.Range(-1.00f, 1.00f), ForceMode2D.Impulse);

        if (otherObj.gameObject.CompareTag("Hoop"))
        {
            tinyExplosion.Play();
            PlaySound("Explosion");
            CameraShaker.Instance.ShakeOnce(5.0f, 5.0f, 0.1f, 1.0f);
            if(GameManager.Instance.vibrationState == true) { Handheld.Vibrate(); }

            UIPopup popup = UIPopup.GetPopup("Points");

            Vector2 textPos = (otherObj.transform.position + Vector3.up);
            popup.transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition = textPos;

            TextMeshProUGUI text = popup.Data.Labels[0].GetComponent<TextMeshProUGUI>();
            text.SetText(string.Format("-{0}", currentPoints.Value));
            text.color = new Color32(246, 114, 128, 255);
            
            GameEventMessage.SendEvent("ResetScore");

            popup.Show();

            if(!hoopTouched) { hoopTouched = true; }
            if (PerfectDunks > 0) {	PerfectDunks = 0; }

            StartCoroutine(DisableMyself());
        }
	}

    private void OnTriggerExit2D(Collider2D otherObj) 
    {
		if (otherObj.CompareTag("Killzone")) { gameObject.SetActive(false); }

        if(GameManager.Instance.gameState != GameState.Playing) { return; }
        
		if (otherObj.CompareTag("Point") && myRigidbody.velocity.y < -0.25f)
        {
            if (hoopTouched) { return; }

            Physics2D.IgnoreCollision(otherObj, myCollider);
            StartCoroutine(DisableMyself());

            auraExplosion.Play();
            rippleEffect.Emit(transform.position);
            if(GameManager.Instance.vibrationState == true) { Handheld.Vibrate(); }

            Color backgoundColor = _Camera.backgroundColor;
            DOTween.Sequence()
                .Append(_Camera.DOColor(new Color32(100, 100, 100, 255), 0.2f))
                .Append(_Camera.DOColor(backgoundColor, 0.2f));

            PlaySound("PerfectDunk");

            GameEventMessage.SendEvent("PhaseCompleted");
        }
    }

    IEnumerator DisableMyself()
    {
        timer = 0;
        myRigidbody.simulated = false;
        mySpriteRenderer.enabled = false;

        yield return new WaitForSeconds(3.5f);
        gameObject.SetActive(false);
    }

    private void PlaySound(string name, float pitch = 1.0f, float volume = 1.0f)
    {
        AudioSource audioSource = Array.Find(audioSources, source => source.name == "AS_" + name);
        if(audioSource == null) { Debug.LogWarning("Sound: (" + name + ") Not Found!"); return; }
        
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
    }
}