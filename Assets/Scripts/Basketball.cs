using UnityEngine;
using System;
using Doozy.Engine;
using Doozy.Engine.UI;
using EZCameraShake;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class Basketball : MonoBehaviour
{
    // Serialized Fields
    [Header("General Settings")]
    [SerializeField] private float speedToHoop = 2.0f;
    [SerializeField] private IntVariable score = null;
    [SerializeField] private IntVariable perfectDunks = null;
    [SerializeField] private IntVariable multiplier = null;
    [SerializeField] private StringListVar cheerfulWords = null;

    [Space(2), Header("Audio FX")]
    [SerializeField] private AudioSource[] audioSources = new AudioSource[0];

    [Space(2), Header("VFX")]
    [SerializeField] private GameObject borderHitFx = null;
    [SerializeField] private GameObject hoopEffect = null;
    [SerializeField] private GameObject hoopRingPerfect = null;
    [SerializeField] private GameObject lightingRay = null;

    // NonSerialized Fields
    private bool hoopTouched = false;
    private Vector2 lastPos = Vector2.zero;
    private Camera _Camera = null;
    private float timer = 0f;

    private CircleCollider2D myCollider = null;
    private Rigidbody2D myRigidbody = null;
    private TrailRenderer trailRenderer = null;
    private Transform hoop = null; 

    // Properties
    private int Score { 
        get { return score.Value; } 
        set 
        {
            score.Value = value; 
            PlaySound("Dunk", UnityEngine.Random.Range(0.6f, 1.2f));
            GameEventMessage.SendEvent("PointScored");
            if(GameManager.Instance.vibrationState == true) { Handheld.Vibrate(); }
        } 
    }

    private int PerfectDunks { 
        get { return perfectDunks.Value; } 
        set 
        { 
            int val = Mathf.Clamp(value, 0, 2);
            if(val < 3)
            {
                perfectDunks.Value = val;
                GameEventMessage.SendEvent("PerfectDunk");

                if(val <= 0) { GameEventMessage.SendEvent("X2TimeOut"); return; }

                CameraShaker.Instance.ShakeOnce(2.0f, 2.0f, 0.1f, 1.0f);
                Color backgroundColor = _Camera.backgroundColor;
                DOTween.Sequence()
                    .Append(_Camera.DOColor(new Color32(100, 100, 100, 255), 0.2f))
                    .Append(_Camera.DOColor(backgroundColor, 0.2f));

                PlaySound("PerfectDunk");
                
                UIPopup popup = UIPopup.GetPopup("SuperWords");
                popup.Data.Labels[0].GetComponent<TextMeshProUGUI>().SetText(cheerfulWords.Value[UnityEngine.Random.Range(0, cheerfulWords.Value.Count)]);
                popup.Show();
            }
        } 
    }

    private int Multiplier { get { return multiplier.Value; } }

    private void Awake() 
    {
        _Camera = Camera.main;
        myCollider = GetComponent<CircleCollider2D>();
        myRigidbody = GetComponent<Rigidbody2D>();
        trailRenderer = GetComponent<TrailRenderer>();
        hoop = GameObject.FindGameObjectWithTag("Hoop").transform;
    }

    private void OnEnable() 
    {
        timer = 0;
        SetEffects();
        hoopTouched = false;
        myRigidbody.AddTorque(UnityEngine.Random.Range(-1.00f, 1.00f), ForceMode2D.Impulse);
    }

    private void Update() {
        if(hoop != null) {
            Vector2 distanceFromHoop = (transform.position - hoop.position);
            float xPos = distanceFromHoop.x;
            float yPos = distanceFromHoop.y;

            if(xPos > -0.4f && xPos < 0.4f && yPos > 0.0f && yPos < 2.0f) {
                transform.position = new Vector2 (
                    Mathf.Lerp(transform.position.x, hoop.position.x, Time.deltaTime * speedToHoop),
                    transform.position.y
                );
            }
        }

        if(transform.position.x == lastPos.x || transform.position.y == lastPos.y) {
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
        myRigidbody.AddTorque(UnityEngine.Random.Range(-1.00f, 1.00f), ForceMode2D.Impulse);

        float soundVolume = 0.0f;
        float impactForce =  KineticEnergy(myRigidbody);
        if      (impactForce <= 7.5f)                       { soundVolume = 0.4f; }
        else if (impactForce > 7.5f && impactForce < 15)    { soundVolume = 0.6f; }
        else if (impactForce >= 15 && impactForce <= 22.5f) { soundVolume = 0.8f; }
        else if (impactForce > 20)                          { soundVolume = 1.0f; }

        if (otherObj.gameObject.CompareTag("Hoop"))
        {
            PlaySound("Hit", UnityEngine.Random.Range(1.2f, 1.6f), soundVolume);
            if(!hoopTouched) { hoopTouched = true; }
            if (PerfectDunks > 0) {
                if(Multiplier != 1) { PerfectDunks = 0; }
            }
        } else {
            PlaySound("Hit", UnityEngine.Random.Range(0.4f, 0.8f), soundVolume);

            if (otherObj.gameObject.CompareTag("Wall")) {
                Instantiate(borderHitFx, new Vector2(otherObj.transform.position.x, transform.position.y), Quaternion.identity);
            }
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

            if (!hoopTouched) 
            { 
                Score += (2 + Multiplier);
                points += (2 + Multiplier); 
                PerfectDunks += 1;
                Instantiate(hoopRingPerfect, otherObj.transform.parent.position, Quaternion.identity);
                Instantiate(lightingRay, new Vector2(otherObj.transform.parent.position.x, 0), Quaternion.identity);
            } 
            else 
            { 
                Score += (1 + Multiplier);
                points += (1 + Multiplier);
            }

            Instantiate(hoopEffect, otherObj.transform.parent.position, Quaternion.identity);

            UIPopup popup = UIPopup.GetPopup("Points");

            Vector2 textPos = (otherObj.transform.position + Vector3.up);
            popup.transform.parent.gameObject.GetComponent<RectTransform>().anchoredPosition = textPos;

            TextMeshProUGUI text = popup.Data.Labels[0].GetComponent<TextMeshProUGUI>();
            text.SetText(string.Format("+{0}", points));
            text.color = new Color32(167, 233, 175, 255);

            popup.Show();

            popup = UIPopup.GetPopup("Words");
            popup.Data.Labels[0].GetComponent<TextMeshProUGUI>().SetText(cheerfulWords.Value[UnityEngine.Random.Range(0, cheerfulWords.Value.Count)]);
            popup.Show();
        }
    }

    public void SetEffects() 
    {
        trailRenderer.enabled = false;
        if (PerfectDunks >= 1) {
            trailRenderer.enabled = true;
        }
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