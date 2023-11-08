using UnityEngine.UI;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class AnimationPlayer : MonoBehaviour
{
    [Flags] enum Functions { 
        Start       = (1 << 0), 
        OnEnable    = (1 << 1), 
        OnTrigger   = (1 << 2), 
        OnCollision = (1 << 3),
        OnDestroy   = (1 << 4),
        OnDisable   = (1 << 5),
    } 

    [EnumToggleButtons, HideLabel]
    [SerializeField] private Functions functions = Functions.Start;

    private bool StartFunct { get { return (functions & Functions.Start) != 0; } }
    private bool OnEnableFunct { get { return (functions & Functions.OnEnable) != 0; } }
    private bool OnTriggerFunct { get { return (functions & Functions.OnTrigger) != 0; } }
    private bool OnCollisionFunct { get { return (functions & Functions.OnCollision) != 0; } }
    private bool OnDestroyFunct { get { return (functions & Functions.OnDestroy) != 0; } }
    private bool OnDisableFunct { get { return (functions & Functions.OnDisable) != 0; } }


    [ShowIfGroup("OnDestroyFunct"), BoxGroup("OnDestroyFunct/Extra Settings", CenterLabel = true)]
    [SerializeField] private float timeToDestroy = 3;

    [ShowIfGroup("OnDisableFunct"), BoxGroup("OnDisableFunct/Extra Settings", CenterLabel = true)]
    [SerializeField] private float timeToDisable = 3;

    [ShowIfGroup("OnDisableFunct"), BoxGroup("OnDisableFunct/Extra Settings")]
    [SerializeField] private float timeToRespawn = 3;


    [ShowIfGroup("StartFunct"), BoxGroup("StartFunct/Start Animation", CenterLabel = true), HideLabel]
    [SerializeField] private TweenAnimation startAnim = null;

    [ShowIfGroup("OnEnableFunct", Functions.OnEnable), BoxGroup("OnEnableFunct/OnEnable Animation", CenterLabel = true), HideLabel]
    [SerializeField] private TweenAnimation onEnableAnim = null;

    [ShowIfGroup("OnTriggerFunct", Functions.OnTrigger), BoxGroup("OnTriggerFunct/OnTrigger Animation", CenterLabel = true), HideLabel]
    [SerializeField] private TweenAnimation onTriggerAnim = null;

    [ShowIfGroup("OnCollisionFunct", Functions.OnCollision), BoxGroup("OnCollisionFunct/OnCollision Animation", CenterLabel = true), HideLabel]
    [SerializeField] private TweenAnimation onCollisionAnim = null;

    private SpriteRenderer spriteRenderer;
    private Image image;

    private void Awake() 
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
        image = this.GetComponent<Image>();
    }

    private void Start()
    {
        if(!StartFunct) return;
        Animate(startAnim);

        if(!OnDestroyFunct) return;
        Destroy(gameObject, timeToDestroy);
    }

    private void OnEnable() 
    {
        if(!OnEnableFunct) return;
        Animate(onEnableAnim);

        if(!OnDestroyFunct) return;
        Destroy(gameObject, timeToDestroy);
    }

    private void OnDisable()
    {
        if(!OnDisableFunct) return;

        Invoke("Respawn", timeToRespawn);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if(!OnTriggerFunct) return;

        Animate(onTriggerAnim);

        if(OnDisableFunct) { Invoke("DisableItSelf", timeToDisable); }
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if(!OnCollisionFunct) return;

        Animate(onCollisionAnim);

        if(OnDisableFunct) { Invoke("DisableItSelf", timeToDisable); }
    }

    private void DisableItSelf()
    {
        gameObject.SetActive(false);
    }

    private void Respawn()
    {
        gameObject.SetActive(true);
    }

    private void Animate(TweenAnimation tween)
    {
        if(spriteRenderer != null) { 
            tween.Play(_TransformTarget: transform, _ColorTarget: spriteRenderer); 
        }
        else if(image != null) { 
            tween.Play(_TransformTarget: transform, _ColorTarget: image); 
        }
        else {
            tween.Play(_TransformTarget: transform);
        }
    }                                 
}
        