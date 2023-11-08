using System;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.ProceduralImage;

[CreateAssetMenu(fileName = "Animation", menuName = "Custom/DOTween/New Animation")]
[InlineEditor]
public class TweenAnimation : ScriptableObject
{
    [SerializeField, Toggle("isActive")] 
    private MoveTween move = new MoveTween();
    public MoveTween Move { get { return move; } }


    [SerializeField, Toggle("isActive")]
    private RotateTween rotate = new RotateTween();
    public RotateTween Rotate { get { return rotate; } }


    [SerializeField, Toggle("isActive")] 
    private ScaleTween scale = new ScaleTween();
    public ScaleTween Scale { get { return scale; } }

    [SerializeField, Toggle("isActive")] 
    private ColorTween color = new ColorTween();
    public ColorTween Color { get { return color; } }

    public void Play(object _TransformTarget = null, object _ColorTarget = null)
    {
        if(_TransformTarget != null)
        {
            if(Move.IsActive) { Move.Play(_TransformTarget); }

            if(Rotate.IsActive) { Rotate.Play(_TransformTarget); }

            if(Scale.IsActive) { Scale.Play(_TransformTarget); }
        }

        if(_ColorTarget != null)
        {
            if(Color.IsActive) { Color.Play(_ColorTarget); }
        }
    }

    public void Stop()
    {
        if(Move.v_Tween != null) { Move.v_Tween.Kill(); }

        if(Rotate.v_Tween != null) { Rotate.v_Tween.Kill(); }

        if(Scale.v_Tween != null) { Scale.v_Tween.Kill(); }

        if(Color.v_Tween != null) { Color.v_Tween.Kill(); }
    }
}


// ----> (Class General Settings) <----
public abstract class GeneralSettings
{
    // --> (Start Tweener Settings Variables) <--
    [HideInInspector] protected TargetType targetType = TargetType.NULL;
    protected TargetType GetTargetTypeOf(string target) {
        if(Enum.TryParse(target, out targetType))
        {
            //Debug.Log(string.Format("Converted Object Type: '{0}' to: '{1}' TargetType Enumeration", target, targetType.ToString()));
            return targetType;
        }
        else
        {
            Debug.LogWarning(string.Format("'{0} Type' is not a member of the TargetType Enumeration", target));
            return TargetType.NULL;
        }
    }

    [HideInInspector] public Tweener v_Tween = null;
    // <-- (End Tweener Settings Variables) -->


    // --> (Start IsActive Variable) <--
    [SerializeField] private bool isActive = false;
    public bool IsActive { get { return isActive; } }
    // <-- (End IsActive Variable) -->


    //) --> (Start Duration Variable) <--
    [MinValue(0)]
    [SerializeField] private float duration = 1;
    public float Duration { get { return duration; } }
    //) <-- (End Duration Variable) -->


    //) --> (Start Delay Variable) <--
    [MinValue(0)]
    [SerializeField] private float delay = 0;
    public float Delay { get { return delay; } }
    //) <-- (End Delay Variable) -->


    //) --> (Start Ease Variable) <--
    [SerializeField] private Ease ease = Ease.Linear;
    public Ease Ease { get { return ease; } }
    
    [ShowIf("Ease", Ease.INTERNAL_Custom), HideLabel, Indent(1)]
    [SerializeField] private AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve AnimationCurve { get { return animationCurve; } }
    //) <-- (End Ease Variable) -->


    // --> (Start Loops Variable) <--
    [MinValue(-1)]
    [SerializeField] private int loops = 1;
    public int Loops { get { return loops; } set { loops = value; } }

    private bool Looping { get { return Loops < 0 || Loops > 1 ? true : false; } }
    
    [ShowIf("Looping"), Indent(1)]
    [SerializeField] private LoopType loopType = LoopType.Restart;
    public LoopType LoopType { get { return loopType; } }
    // <-- (End Loops Variable) -->


    // --> (Start Extras Enum Variable) <--
    [HorizontalGroup("SplitEnum"), EnumToggleButtons, HideLabel, Indent(0)]
    [SerializeField] private Extras extras = Extras.IsRelative & (~ Extras.IgnoreTimeScale);
    // <-- (End Extras Enum Variable) -->


    // --> (Start IsRelative Variable) <--
    public bool IsRelative { get { return (extras & Extras.IsRelative) != 0; } }
    // <-- (End IsRelative Variable) -->


    // --> (Start IgnoreTimeScale Variable) <--
    public bool IgnoreTimeScale { get { return (extras & Extras.IgnoreTimeScale) != 0; } }
    // <-- (End IgnoreTimeScale Variable) -->
}

public enum TargetType { NULL, Transform, RectTransform, Rigidbody, Rigidbody2D, Renderer, Light, SpriteRenderer, Image, ProceduralImage, Text, TextMeshProUGUI, TextMeshPro, CanvasGroup }

public enum Space { World, Local }

[Flags] public enum Extras
{
    IsRelative      = (1 << 0),
    IgnoreTimeScale = (1 << 1)
}
// <---- (Class General Settings) ---->


// ----> (Class Move Tween) <----
[Serializable, Toggle("isActive")]
public class MoveTween : GeneralSettings 
{
    // --> (Start Space Variable) <--
    [EnumToggleButtons, OnValueChanged("SetUseV3")]
    [SerializeField] private Space space = Space.World;
    public Space Space { get { return space; } }

    private void SetUseV3() { if(useTarget) { useTarget = false; } }
    // <-- (End Space Variable) -->


    // --> (Start IsFrom Variable) <--
    [HideInInspector]
    [SerializeField] private bool isFrom = false;
    public bool IsFrom { get { return isFrom; } }

    [HorizontalGroup("Split", Width = 0.125f)]

    [ShowIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left")]
    private void From() { isFrom = !isFrom; } 

    [HideIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left")]
    private void To() { isFrom = !isFrom; } 
    // <-- (End IsFrom Variable) -->


    // --> (Start End Value Variable) <--
    [HorizontalGroup("Split", MarginLeft = 0.07f)]
    [HideIf("UseTarget"), VerticalGroup("Split/Right", 1), HideLabel]
    [SerializeField] private Vector3 endValueV3 = Vector3.zero;
    public Vector3 EndValueV3 { get { return endValueV3; } }

    [ShowIf("UseTarget"), VerticalGroup("Split/Right"), HideLabel]
    [SerializeField] private Transform endValueTransform = null;
    public Transform EndValueTransform { get { return endValueTransform; } }
    // <-- (End End Value Variable) -->


    // --> (Start use Target as V3 Variable) <--
    private bool useTarget = false;
    public bool UseTarget { get { return useTarget; } }

    [HorizontalGroup("Split", Width = 0.125f)]
    [ShowIfGroup("Split/Space", Space.World)]
    [ShowIf("UseTarget"), Button(ButtonSizes.Small), VerticalGroup("Split/Space/One")]
    private void Target() { useTarget = !useTarget; } 

    [HideIf("UseTarget"), Button(ButtonSizes.Small), VerticalGroup("Split/Space/One")]
    private void Value() { useTarget = !useTarget; } 
    // <-- (End use Target as V3 Variable) -->


    // --> (Start Snapping Variable) <--
    [Flags] private enum OptionalBool { Snapping = (1 << 0) }

    [HorizontalGroup("SplitEnum", Width = 0.265f), EnumToggleButtons, HideLabel]
    [SerializeField] private OptionalBool optionalBool = (~OptionalBool.Snapping);
    public bool Snapping { get { return (optionalBool & OptionalBool.Snapping) != 0; } }
    // <-- (End Snapping Variable) -->


    // --> (Start Play Function) <--
    public void Play(object target)
    {
        GameObject _ObjectCast = target as GameObject;

        switch (Space)
        {
            case Space.World:
                if(UseTarget)
                {

                    if(EndValueTransform == null)
                    {
                        Debug.LogWarning(string.Format("{0} :: This tween's TO target is NULL, a Vector3 of (0,0,0) will be used instead", _ObjectCast.name), _ObjectCast);
                        endValueV3 = Vector3.zero;
                    }
                    else
                    {
                        if(target.GetType() == typeof(RectTransform))
                        {
                            RectTransform endValueRectT = endValueTransform as RectTransform;
                            if(endValueRectT == null)
                            {
                                Debug.LogWarning(string.Format("{0} :: This tween's TO target should be a RectTransform, a Vector3 of (0,0,0) will be used instead", _ObjectCast.name), _ObjectCast);
                                endValueV3 = Vector3.zero;
                            }
                            else
                            {
                                RectTransform rectTarget = target as RectTransform; 
                                if(rectTarget == null)
                                {
                                    Debug.LogWarning(string.Format("{0} :: This tween's target and TO target are not of the same type. Please reassign the values", _ObjectCast.name), _ObjectCast);
                                }
                                else
                                {
                                    endValueV3 = DOTweenModuleUI.Utils.SwitchToRectTransform(endValueRectT, rectTarget);
                                }
                            }
                        }
                        else
                        {
                            endValueV3 = EndValueTransform.position;
                        }
                    }
                }

                var n_TargetType = GetTargetTypeOf(target.GetType().Name);
                switch (n_TargetType)
                {
                    case TargetType.Transform:
                        v_Tween = ((Transform)target).DOMove(EndValueV3, Duration, Snapping);
                    break;

                    case TargetType.RectTransform:
                        v_Tween = ((RectTransform)target).DOAnchorPos3D(EndValueV3, Duration, Snapping);
                        v_Tween = ((Transform)target).DOMove(EndValueV3, Duration, Snapping);
                    break;

                    case TargetType.Rigidbody:
                        v_Tween = ((Rigidbody)target).DOMove(EndValueV3, Duration, Snapping);
                        v_Tween = ((Transform)target).DOMove(EndValueV3, Duration, Snapping);
                    break;

                    case TargetType.Rigidbody2D:
                        v_Tween = ((Rigidbody2D)target).DOMove(EndValueV3, Duration, Snapping);
                        v_Tween = ((Transform)target).DOMove(EndValueV3, Duration, Snapping);
                    break;

                    default:
                        Debug.LogWarning(string.Format("{0} :: This Target Type is not valid in the given context. Please reassign the value", n_TargetType.ToString()), _ObjectCast);
                    break;
                }
            break;

            case Space.Local:
                if(target.GetType() == typeof(Transform) || target.GetType() == typeof(RectTransform))
                    v_Tween = (target as Transform).DOLocalMove(EndValueV3, Duration, Snapping);
                else{
                    Debug.LogWarning(string.Format("{0} :: This Target Type is not valid in the given context. Please reassign the value", target.GetType().ToString()), _ObjectCast);
                    return;
                }
            break;
        }

        if (v_Tween == null) { Debug.LogWarning("v_Tween in " + target + " is null."); return; } 

        if (IsFrom) { 
            ((Tweener)v_Tween).From(IsRelative); 
        }
        else { 
            v_Tween.SetRelative(IsRelative); 
        }

        v_Tween.SetTarget(target).SetDelay(Delay).SetLoops(Loops, LoopType).OnKill(()=> v_Tween = null);

        if(Ease == Ease.INTERNAL_Custom) {
            v_Tween.SetEase(AnimationCurve);
        } 
        else {
            v_Tween.SetEase(Ease);
        }

        v_Tween.SetUpdate(IgnoreTimeScale);

        v_Tween.Play();
    }
    // <-- (End Play Function) -->
}
// <---- (Class Move Tween) ---->


// ----> (Class Rotate Tween) <----
[Serializable, Toggle("isActive")]
public class RotateTween : GeneralSettings 
{ 
    // --> (Start Space Variable) <--
    [EnumToggleButtons]
    [SerializeField] private Space space = Space.World;
    public Space Space { get { return space; } }
    // <-- (End Space Variable) -->


    // --> (Start IsFrom Variable) <--
    [HideInInspector]
    [SerializeField] private bool isFrom = false;
    public bool IsFrom { get { return isFrom; } }

    [HorizontalGroup("Split", Width = 0.15f)]
    [ShowIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left", 0)]
    public void From() { isFrom = !isFrom; } 

    [HideIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left")]
    public void To() { isFrom = !isFrom; } 
    // <-- (End IsFrom Variable) -->


    // --> (Start End Value Variable) <--
    [HorizontalGroup("Split", MarginLeft = 0.175f)]
    [VerticalGroup("Split/Right", 2), HideLabel]
    [SerializeField] private Vector3 endValueV3;
    public Vector3 EndValueV3 { get { return endValueV3; } set { endValueV3 = value; } }
    // <-- (End End Value Variable) -->


    // --> (Start Snapping Variable) <--
    [HorizontalGroup("SplitEnum", Width = 0.275f), HideLabel]
    [SerializeField] private RotateMode rotationMode = RotateMode.Fast;
    public RotateMode RotationMode { get { return rotationMode; } }
    // <-- (End Snapping Variable) -->


    // --> (Start Play Function) <--
    public void Play(object target)
    {
        GameObject _ObjectCast = target as GameObject;

        switch (Space)
        {
            case Space.World:
                var n_TargetType = GetTargetTypeOf(target.GetType().Name);
                switch (n_TargetType)
                {
                    case TargetType.Transform:
                        v_Tween = ((Transform)target).DORotate(EndValueV3, Duration, RotationMode);
                    break;

                    case TargetType.Rigidbody:
                        v_Tween = ((Rigidbody)target).DORotate(EndValueV3, Duration, RotationMode);
                        v_Tween = ((Transform)target).DORotate(EndValueV3, Duration, RotationMode);
                    break;

                    case TargetType.Rigidbody2D:
                        v_Tween = ((Rigidbody2D)target).DORotate(EndValueV3.z, Duration);
                        v_Tween = ((Transform)target).DORotate(EndValueV3, Duration, RotationMode);
                    break;

                    default:
                        Debug.LogWarning(string.Format("{0} :: This Target Type is not valid in the given context. Please reassign the value", n_TargetType.ToString()), _ObjectCast);
                    break;
                }
            break;

            case Space.Local:
                if(target.GetType() == typeof(Transform) || target.GetType() == typeof(RectTransform))
                    v_Tween = (target as Transform).DOLocalRotate(EndValueV3, Duration, RotationMode);
                else{
                    Debug.LogWarning(string.Format("{0} :: This Target Type is not valid in the given context. Please reassign the value", target.GetType().ToString()), _ObjectCast);
                    return;
                }
            break;
        }

        if (v_Tween == null) { Debug.LogWarning("v_Tween in " + target + " is null."); return; } 

        if (IsFrom) { 
            ((Tweener)v_Tween).From(IsRelative); 
        }
        else { 
            v_Tween.SetRelative(IsRelative); 
        }

        v_Tween.SetTarget(target).SetDelay(Delay).SetLoops(Loops, LoopType).OnKill(()=> v_Tween = null);

        if(Ease == Ease.INTERNAL_Custom) {
            v_Tween.SetEase(AnimationCurve);
        } 
        else {
            v_Tween.SetEase(Ease);
        }

        v_Tween.SetUpdate(IgnoreTimeScale);

        v_Tween.Play();
    }
    // <-- (End Play Function) -->
}
// <---- (Class Rotate Tween) ---->


// ----> (Class Scale Tween) <----
[Serializable, Toggle("isActive")]
public class ScaleTween : GeneralSettings 
{
    // --> (Start IsFrom Variable) <--
    [HideInInspector]
    [SerializeField] private bool isFrom = false;
    public bool IsFrom { get { return isFrom; } }

    [HorizontalGroup("Split", Width = 0.15f)]
    [ShowIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left", 0)]
    public void From() { isFrom = !isFrom; } 

    [HideIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left")]
    public void To() { isFrom = !isFrom; } 
    // <-- (End IsFrom Variable) -->


    // --> (Start End Value Variable) <--
    [HorizontalGroup("Split", MarginLeft = 0.175f)]
    [HideIf("UniformScale"), VerticalGroup("Split/Right", 2), HideLabel]
    [SerializeField] private Vector3 endValueV3 = Vector3.zero;
    public Vector3 EndValueV3 { get { return endValueV3; } set { endValueV3 = value; } }

    [ShowIf("UniformScale"), VerticalGroup("Split/Right"), HideLabel]
    [SerializeField] private float endValueFloat = 0;
    public float EndValueFloat { get { return endValueFloat; } set { endValueFloat = value; } }
    // <-- (End End Value Variable) -->


    // --> (Start UniformScale Variable) <--
    [Flags] private enum OptionalBool { UniformScale = (1 << 0) }

    [HorizontalGroup("SplitEnum", Width = 0.265f), EnumToggleButtons, HideLabel]
    [SerializeField] private OptionalBool optionalBool = ~OptionalBool.UniformScale;
    public bool UniformScale { get { return (optionalBool & OptionalBool.UniformScale) != 0; } }
    // <-- (End UniformScale Variable) -->


    // --> (Start UniformVector Function) <--
    private Vector3 UniformVector(float n)
    {
        return new Vector3(n, n, n);
    } 
    // <-- (Start UniformVector Function) -->


    // --> (Start Play Function) <--
    public void Play(object target)
    {
        GameObject _ObjectCast = target as GameObject;

        if(target.GetType() == typeof(Transform) || target.GetType() == typeof(RectTransform))
            v_Tween = (target as Transform).DOScale(UniformScale ? UniformVector(EndValueFloat) : EndValueV3, Duration);
        else{
            Debug.LogWarning(string.Format("{0} :: This Target Type is not valid in the given context. Please reassign the value", target.GetType().ToString()), _ObjectCast);
            return;
        }

        if (v_Tween == null) { Debug.LogWarning("v_Tween in " + target + " is null."); return; } 

        if (IsFrom) { 
            ((Tweener)v_Tween).From(IsRelative); 
        }
        else { 
            v_Tween.SetRelative(IsRelative); 
        }

        v_Tween.SetTarget(target).SetDelay(Delay).SetLoops(Loops, LoopType).OnKill(()=> v_Tween = null);

        if(Ease == Ease.INTERNAL_Custom) {
            v_Tween.SetEase(AnimationCurve);
        } 
        else {
            v_Tween.SetEase(Ease);
        }

        v_Tween.SetUpdate(IgnoreTimeScale);

        v_Tween.Play();
    }
    // <-- (End Play Function) -->
}
// <---- (Class Scale Tween) ---->


// ----> (Class Color Tween) <----
[Serializable, Toggle("isActive")]
public class ColorTween : GeneralSettings 
{ 
    // --> (Start ColorType Variable) <--
    public enum ColorType { Color, Fade }
    [EnumToggleButtons, HideLabel]
    [SerializeField] private ColorType colorType = ColorType.Color;
    public ColorType ColorTypeProperty { get { return colorType; } }
    // <-- (End ColorType Variable) -->


    // --> (Start IsFrom Variable) <--
    [HideInInspector]
    [SerializeField] private bool isFrom = false;
    public bool IsFrom { get { return isFrom; } }

    [HorizontalGroup("Split", Width = 0.15f)]
    [ShowIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left", 0)]
    public void From() { isFrom = !isFrom; } 

    [HideIf("IsFrom"), Button(ButtonSizes.Small), VerticalGroup("Split/Left")]
    public void To() { isFrom = !isFrom; } 
    // <-- (End IsFrom Variable) -->


    // --> (Start End Value Variable) <--
    [HorizontalGroup("Split", MarginLeft = 0.175f)]
    [ShowIf("ColorTypeProperty", ColorType.Color), VerticalGroup("Split/Right", 2), HideLabel]
    [SerializeField] private Color endValueColor = Color.white;
    public Color EndValueColor { get { return endValueColor; } }

    [ShowIf("ColorTypeProperty", ColorType.Fade), VerticalGroup("Split/Right"), HideLabel]
    [SerializeField] private float endValueFloat = 0;
    public float EndValueFloat { get { return endValueFloat; } }
    // <-- (End End Value Variable) -->


    // --> (Start Play Function) <--
    public void Play(object target)
    {
        GameObject _ObjectCast = target as GameObject;

        var n_TargetType = GetTargetTypeOf(target.GetType().Name);
        switch (ColorTypeProperty)
        {
            case ColorType.Color:
                    switch (n_TargetType)
                    {
                        case TargetType.Renderer:
                            v_Tween = ((Renderer)target).material.DOColor(EndValueColor, Duration);
                        break;

                        case TargetType.Light:
                            v_Tween = ((Light)target).DOColor(EndValueColor, Duration);
                        break;

                        case TargetType.SpriteRenderer:
                            v_Tween = ((SpriteRenderer)target).DOColor(EndValueColor, Duration);
                        break;

                        case TargetType.Image:
                            v_Tween = ((Image)target).DOColor(EndValueColor, Duration);
                        break;

                        case TargetType.ProceduralImage:
                            v_Tween = ((ProceduralImage)target).DOColor(EndValueColor, Duration);
                        break;

                        case TargetType.Text:
                            v_Tween = ((Text)target).DOColor(EndValueColor, Duration);
                        break;

                        case TargetType.TextMeshPro:
                            v_Tween = ((TextMeshPro)target).DOColor(EndValueColor, Duration);
                        break;

                        case TargetType.TextMeshProUGUI:
                            v_Tween = ((TextMeshProUGUI)target).DOColor(EndValueColor, Duration);
                        break;

                        default:
                            Debug.LogWarning(string.Format("{0} :: This Target Type is not valid in the given context. Please reassign the value", n_TargetType.ToString()), _ObjectCast);
                        break;
                    }
            break;

            case ColorType.Fade:
                    switch (n_TargetType)
                    {
                        case TargetType.Renderer:
                            v_Tween = ((Renderer)target).material.DOFade(EndValueFloat, Duration);
                        break;

                        case TargetType.Light:
                            v_Tween = ((Light)target).DOIntensity(EndValueFloat, Duration);
                        break;

                        case TargetType.SpriteRenderer:
                            v_Tween = ((SpriteRenderer)target).DOFade(EndValueFloat, Duration);
                        break;

                        case TargetType.Image:
                            v_Tween = ((Image)target).DOFade(EndValueFloat, Duration);
                        break;

                        case TargetType.ProceduralImage:
                            v_Tween = ((ProceduralImage)target).DOFade(EndValueFloat, Duration);
                        break;

                        case TargetType.Text:
                            v_Tween = ((Text)target).DOFade(EndValueFloat, Duration);
                        break;

                        case TargetType.CanvasGroup:
                            v_Tween = ((CanvasGroup)target).DOFade(EndValueFloat, Duration);
                        break;

                        case TargetType.TextMeshPro:
                            v_Tween = ((TextMeshPro)target).DOFade(EndValueFloat, Duration);
                        break;

                        case TargetType.TextMeshProUGUI:
                            v_Tween = ((TextMeshProUGUI)target).DOFade(EndValueFloat, Duration);
                        break;

                        default:
                            Debug.LogWarning(string.Format("{0} :: This Target Type is not valid in the given context. Please reassign the value", n_TargetType.ToString()), _ObjectCast);
                        break;
                    }
            break;
        }

        if (v_Tween == null) { Debug.LogWarning("v_Tween in " + target + " is null."); return; } 

        if (IsFrom) { 
            ((Tweener)v_Tween).From(IsRelative); 
        }
        else { 
            v_Tween.SetRelative(IsRelative); 
        }

        v_Tween.SetTarget(target).SetDelay(Delay).SetLoops(Loops, LoopType).OnKill(()=> v_Tween = null);

        if(Ease == Ease.INTERNAL_Custom) {
            v_Tween.SetEase(AnimationCurve);
        } 
        else {
            v_Tween.SetEase(Ease);
        }

        v_Tween.SetUpdate(IgnoreTimeScale);

        v_Tween.Play();
    }
    // <-- (End Play Function) -->
}
// <---- (Class Color Tween) ---->
