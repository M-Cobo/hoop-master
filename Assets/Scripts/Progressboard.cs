using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.UI;
using System;
using Doozy.Engine;

public class Progressboard : MonoBehaviour
{
    [TitleGroup("UI Board Settings", Alignment = TitleAlignments.Centered)]
    
    [LabelText("Points Container")]
    [SerializeField] private GameObject UI_ScoreContainer = null;

    [LabelText("Point Prefab"), Indent(1)]
    [SerializeField] private GameObject UI_PointPrefab = null;

    [LabelText("Phases Container")]
    [SerializeField] private GameObject UI_PhasesContainer = null;

    [LabelText("Phase Prefab"), Indent(1)]
    [SerializeField] private GameObject UI_PhasePrefab = null;

    [LabelText("Level Prefab"), Indent(1)]
    [SerializeField] private GameObject UI_LevelPrefab = null;

    [LabelText("Finish Prefab"), Indent(1)]
    [SerializeField] private GameObject UI_FinishPrefab = null;


    [TitleGroup("Current Level Setup", Alignment = TitleAlignments.Centered)]

    [LabelText("Level Phases")] [SerializeField] private IntVariable v_Phases = null;  
    [LabelText("Current Phase")] [SerializeField] private IntVariable c_Phases = null;  

    [LabelText("Phase Points")]
    [SerializeField] private IntVariable v_Points = null; 


    [FoldoutGroup("Animations Settings")]
    
    [FoldoutGroup("Animations Settings/Progressboard - UI")]
    [SerializeField] private float delayToShow = 1;
    [FoldoutGroup("Animations Settings/Progressboard - UI")]
    [SerializeField] private TweenAnimation phaseboard = null;
    [FoldoutGroup("Animations Settings/Progressboard - UI")]
    [SerializeField] private TweenAnimation scoreboardIn = null;
    [FoldoutGroup("Animations Settings/Progressboard - UI")]
    [SerializeField] private TweenAnimation scoreboardOut = null;

    [FoldoutGroup("Animations Settings/Basketballs - UI")]
    [SerializeField] private TweenAnimation basketballIn = null;
    [FoldoutGroup("Animations Settings/Basketballs - UI")]
    [SerializeField] private TweenAnimation ui_PhaseIn = null;
    [FoldoutGroup("Animations Settings/Phases - UI"), LabelText("UIPhase Out")]
    [SerializeField] private TweenAnimation ui_PhaseOut = null;
    [FoldoutGroup("Animations Settings/Phases - UI")]
    [SerializeField] private TweenAnimation textToBlack = null;
    [FoldoutGroup("Animations Settings/Phases - UI")]
    [SerializeField] private TweenAnimation backgroundToWhite = null;
    [FoldoutGroup("Animations Settings/Phases - UI")]
    [SerializeField] private TweenAnimation checkFadeIn = null;
    [FoldoutGroup("Animations Settings/Phases - UI")]
    [SerializeField] private TweenAnimation textFadeOut = null;


    // Non Serialized Vars
    private List<PhaseGraphic> phaseGraphics = new List<PhaseGraphic>();
    private List<GameObject> balls = new List<GameObject>();

    private void Start() 
    {
        CreateProgressbar();
    }

    private void CreateProgressbar()
    {
        SetPhasesboard();
        SetScoreboard();
    }

    private void SetPhasesboard()
    {
        GameObject a_LevelHolder = Instantiate (
            UI_LevelPrefab,
            Vector2.zero,
            Quaternion.identity
        );

        a_LevelHolder.SetActive(true);
        TextMeshProUGUI levelText = a_LevelHolder.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        levelText.SetText(PlayerPrefs.GetInt("CurrentLevel", 1).ToString());

        a_LevelHolder.transform.SetParent(UI_PhasesContainer.transform);

        phaseboard.Play(_TransformTarget: a_LevelHolder.transform);

        for (int i = 0; i < v_Phases.Value; i++)
        {
            GameObject a_Phase = Instantiate (
                UI_PhasePrefab,
                Vector2.zero,
                Quaternion.identity
            );
            
            Image phaseHolderImage = a_Phase.transform.GetChild(0).GetComponent<Image>();
            Image phaseCheckmark = a_Phase.transform.GetChild(0).Find("Check").gameObject.GetComponent<Image>();
            TextMeshProUGUI phaseText = a_Phase.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

            phaseCheckmark.color = new Color(1, 1, 1, 0);
            phaseText.SetText((i + 1).ToString());

            PhaseGraphic phaseGraphic;

            if(i == 0)
            {
                a_Phase.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                phaseText.color = new Color32(100, 100, 100, 255);

                phaseHolderImage.color = Color.white;

                phaseGraphic = new PhaseGraphic(true, a_Phase, phaseHolderImage, phaseCheckmark, phaseText);
            }
            else { phaseGraphic = new PhaseGraphic(false, a_Phase, phaseHolderImage, phaseCheckmark, phaseText); }

            phaseGraphics.Add(phaseGraphic);

            a_Phase.transform.SetParent(UI_PhasesContainer.transform);

            phaseboard.Play(_TransformTarget: a_Phase.transform);
        }

        GameObject a_FinishHolder = Instantiate (
            UI_FinishPrefab,
            Vector2.zero,
            Quaternion.identity
        );

        a_FinishHolder.SetActive(true);
        Image finishHolderImage = a_FinishHolder.transform.GetChild(0).GetComponent<Image>();
        Image levelFinish = a_FinishHolder.transform.GetChild(0).Find("Finish").gameObject.GetComponent<Image>();

        PhaseGraphic finishGraphic = new PhaseGraphic(false, a_FinishHolder, finishHolderImage, levelFinish, null);
        phaseGraphics.Add(finishGraphic);

        a_FinishHolder.transform.SetParent(UI_PhasesContainer.transform);

        phaseboard.Play(_TransformTarget: a_FinishHolder.transform);
    }
    
    public void SetScoreboard() { if(c_Phases.Value < v_Phases.Value){ StartCoroutine(ScoreboardSetup()); } }

    IEnumerator ScoreboardSetup()
    {

        scoreboardOut.Play(_TransformTarget: UI_ScoreContainer.transform);

        yield return new WaitForSeconds(delayToShow);
        yield return new WaitUntil (() => GameManager.Instance.gameState != GameState.Waiting);
        
        if(balls.Count > 0) {
            foreach (GameObject ballSpot in balls)
            {
                Destroy(ballSpot.transform.parent.gameObject);
            }

            balls.Clear();
        }

        RectTransform rect = UI_ScoreContainer.GetComponent<RectTransform>();
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 60);

        for (int i = 0; i < v_Points.Value; i++)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.sizeDelta.x + 45);

            GameObject n_Point = Instantiate (
                UI_PointPrefab,
                Vector2.zero,
                Quaternion.identity
            );

            n_Point.transform.SetParent(UI_ScoreContainer.transform);

            balls.Add(n_Point.transform.Find("Ball").gameObject);
        }

        scoreboardIn.Play(_TransformTarget: UI_ScoreContainer.transform);

        GameEventMessage.SendEvent("Is Playing");
    }

    public void ResetScore(IntVariable Points)
    {
        for (int i = 0; i < Points.Value; i++) {
            if(balls[i].activeInHierarchy) {
                balls[i].SetActive(false);
            }
        }

        Points.Value = 0;
    }

    public void UpdateScoreboard(IntVariable Points)
    {   
        if(Points.Value >= v_Points.Value) { 
            Points.Value = Mathf.Clamp(Points.Value, 0, v_Points.Value);
            GameManager.Instance.SetGameState(GameState.Waiting);
            GameEventMessage.SendEvent("PhaseCompleted");
        }

        for (int i = 0; i < Points.Value; i++)
        {
            if(!balls[i].activeInHierarchy)
            {
                balls[i].SetActive(true);
                basketballIn.Play(_TransformTarget: balls[i].transform);
            }
        }
    }

    public void UpdatePhase(IntVariable Phase)
    {
        int a_Phase = Phase.Value;
        
        // Change state of previous phase to "complete"
        if(a_Phase > 0) {
            if(phaseGraphics[a_Phase - 1].On) {
                phaseGraphics[a_Phase - 1].Change(
                    false,
                    ui_PhaseOut,
                    null,
                    textFadeOut,
                    checkFadeIn
                );
            }
        }

        // Change state of actual phase to "playing now"
        if((a_Phase) < (v_Phases.Value)) {
            if(!phaseGraphics[a_Phase].On) {
                phaseGraphics[a_Phase].Change(
                    true,
                    ui_PhaseIn,
                    backgroundToWhite,
                    textToBlack
                );
            }
        }

        // Check the Finish Goal 
        if((a_Phase) == (v_Phases.Value)) {
            phaseGraphics[a_Phase].Change(
                true,
                ui_PhaseIn,
                backgroundToWhite,
                null,
                checkFadeIn
            );
        }
    }
}

[Serializable]
public class PhaseGraphic
{
    public bool On;
    public GameObject GameObject;
    public Image HolderImage;
    public Image CheckmarkImage;
    public TextMeshProUGUI TextMeshPro;

    public PhaseGraphic(bool on, GameObject gameObject, Image holderImage, Image checkmark, TextMeshProUGUI textMeshPro)
    {
        this.On = on;
        this.GameObject = gameObject;
        this.HolderImage = holderImage;
        this.CheckmarkImage = checkmark;
        this.TextMeshPro = textMeshPro;
    }

    public void Change(bool activate, TweenAnimation animation, TweenAnimation imageColorTo = null, TweenAnimation textColorTo = null, TweenAnimation checkmarkColor = null)
    {
        On = activate;

        animation.Play(_TransformTarget: this.GameObject.transform);

        if(imageColorTo != null) 
        {
            imageColorTo.Play(_ColorTarget: HolderImage);
        }

        if(textColorTo != null)
        {
            textColorTo.Play(_ColorTarget: TextMeshPro);
        }

        if(checkmarkColor != null)
        {
            checkmarkColor.Play(_ColorTarget: CheckmarkImage);
        }
    }
}
