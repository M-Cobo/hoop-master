using UnityEngine;
using Doozy.Engine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Doozy.Engine;
using Doozy.Engine.Progress;
using System.Collections;
using EZCameraShake;

public class MenusManager : MonoBehaviour {

    #region Serialized Fields
    [TabGroup("MainMenu Settings"), LabelText("Level Text")]
    [SerializeField] private TextMeshProUGUI m_LevelText = null;
    [TabGroup("MainMenu Settings"), LabelText("Coins Text")]
    [SerializeField] private TextMeshProUGUI m_CoinsText = null;
    [TabGroup("MainMenu Settings")]
    [SerializeField] private UIToggle vibrationToggle = null;
    [TabGroup("MainMenu Settings")]
    [SerializeField] private UIToggle soundToggle = null;
    [TabGroup("MainMenu Settings")]
    [SerializeField] private UnityEngine.Audio.AudioMixer audioMixer = null;

    [TabGroup("GameOver Settings"), LabelText("Title Transform")]
    [SerializeField] private RectTransform go_TitleTransform = null;
    [TabGroup("GameOver Settings"), LabelText("Level Text")]
    [SerializeField] private TextMeshProUGUI go_LevelText = null;
    [TabGroup("GameOver Settings"), LabelText("Color Speed")]
    [SerializeField] private float go_TextColorSpeed = 1.0f;
    [TabGroup("GameOver Settings"), LabelText("Title Animation")]
    [SerializeField] private TweenAnimation go_TitleAnim = null;
    [TabGroup("GameOver Settings"), LabelText("Coins Text")]
    [SerializeField] private TextMeshProUGUI go_CoinsText = null;
    [TabGroup("GameOver Settings"), LabelText("Coins Progressor")]
    [SerializeField] private Progressor go_CoinsProgressor = null;
    [TabGroup("GameOver Settings")]
    [SerializeField] private IntVariable collectedCoins = null;
    [TabGroup("GameOver Settings"), LabelText("Fireworks")]
    [SerializeField] private GameObject go_Fireworks = null;

    [BoxGroup("TwoInRow Settings"), LabelText("X2 Timer Progressor")]
    [SerializeField] private Progressor x2_Timer = null;
    [BoxGroup("TwoInRow Settings")]
    [SerializeField] private IntVariable perfectDunks = null;
    [BoxGroup("TwoInRow Settings"), LabelText("Multiplier")]
    [SerializeField] private IntVariable nMultiplier = null;

    [BoxGroup("Phase Cleared Popup"), LabelText("Fireworks")]
    [SerializeField] private GameObject phase_Fireworks = null;
    #endregion

    #region NonSerialized Fields
    private float timeStart;
    #endregion

    private void Start()
    {
        m_LevelText.SetText(PlayerPrefs.GetInt("CurrentLevel", 1).ToString());
        m_CoinsText.SetText(PlayerPrefs.GetInt("Coins").ToString());
        go_LevelText.SetText(PlayerPrefs.GetInt("CurrentLevel", 1).ToString());

        if(PlayerPrefsX.GetBool("Vibration", true))
            vibrationToggle.ToggleOn();
        else 
            vibrationToggle.ToggleOff();

        if(PlayerPrefsX.GetBool("SoundFX", true)){
            soundToggle.ToggleOn();
            audioMixer.SetFloat("masterVol", -20.0f);
        }
        else {
            soundToggle.ToggleOff();
            audioMixer.SetFloat("masterVol", -80.0f);
        }

        timeStart = Time.time;
    }

    private void Update()
    {
        if(GameManager.Instance.gameState == GameState.GameOver) {
            float t = (Mathf.Cos(Time.time - timeStart) * go_TextColorSpeed);
            Color colorTop = Color.Lerp(Color.cyan, Color.green, t);
            Color colorBottom = Color.Lerp(Color.green, Color.cyan, t);
            go_LevelText.colorGradient = new VertexGradient(colorTop, colorTop, colorBottom, colorBottom);
        }
    }
    
    #region GameEvents Listener
    private void OnEnable() {
        //Start listening for game events
        Message.AddListener<GameEventMessage>(OnMessage);
    }

    private void OnDisable() {
        //Stop listening for game events
        Message.RemoveListener<GameEventMessage>(OnMessage);
    }

    private void OnMessage(GameEventMessage message) 
    {
        if(message.EventName == "GameOver") { StartCoroutine(GameOver()); }
        if(message.EventName == "X2TimeOut") { DeactivateX2Timer(); }
    }
    #endregion

    public void TurnVibration(bool value)
    {
        if(value == true) {
            Handheld.Vibrate();
            CameraShaker.Instance.ShakeOnce(3.0f, 3.0f, 0.1f, 1.0f);
        }

        PlayerPrefsX.SetBool("Vibration", value);
        GameManager.Instance.vibrationState = value;
    }

    public void TurnSound(bool value)
    {
        if(value == true)
            audioMixer.SetFloat("masterVol", -20.0f);
        else
            audioMixer.SetFloat("masterVol", -80.0f);

        PlayerPrefsX.SetBool("SoundFX", value);
    }

    public void ShowPopup(string name)
    {
        UIPopup popup = UIPopup.GetPopup(name);

        if(popup != null) {
            popup.Show();
        } else {
            Debug.LogWarning(string.Format("UIPopup : {0}, was not found! Please assign a valid name.", name), gameObject);
        }
    }

    private IEnumerator GameOver()
    {
        go_TitleAnim.Play(_TransformTarget: go_TitleTransform);
        go_TitleAnim.Play(_TransformTarget: go_LevelText.rectTransform);

        go_CoinsText.SetText(PlayerPrefs.GetInt("Coins").ToString());
        if(collectedCoins.Value > 0) {
            StartCoroutine(AddCoins());
        }

        Instantiate(go_Fireworks, new Vector2(3, 4), Quaternion.identity);
        yield return new WaitForSeconds(1.0f);
        Instantiate(go_Fireworks, new Vector2(-3, 1), Quaternion.identity);
    }

    private IEnumerator AddCoins()
    {
        int currentCoins = PlayerPrefs.GetInt("Coins");
        int coinsCollect = collectedCoins.Value;

        while (coinsCollect > 0)
        {
            go_CoinsProgressor.SetProgress(1f);

            yield return new WaitUntil(() => go_CoinsProgressor.GetProgress(TargetProgress.Progress) >= 1f);

            currentCoins += 1; 
            go_CoinsText.SetText(currentCoins.ToString());
            coinsCollect -= 1;
            go_CoinsProgressor.SetProgress(0f);

            yield return new WaitUntil(() => go_CoinsProgressor.GetProgress(TargetProgress.Progress) <= 0f);
        }
    }

    public void ShowLevelCleared()
    {
        if (GameManager.Instance.gameState == GameState.GameOver) { return; }

        UIPopup popup = UIPopup.GetPopup("LevelCleared");
        popup.Show();

        Instantiate(phase_Fireworks, new Vector2(-2, 6), Quaternion.identity);
        Instantiate(phase_Fireworks, new Vector2(2, 6), Quaternion.identity);
        Instantiate(phase_Fireworks, Vector2.zero, Quaternion.identity);
    }

    public void ActivateX2Timer()
    {
        if(perfectDunks.Value == 2 && GameManager.Instance.gameState != GameState.Waiting)
        {
            if(nMultiplier.Value != 1) {
                UIPopup popup = UIPopup.GetPopup("X2PowerUp");
                popup.Show();
            }

            x2_Timer.InstantSetProgress(1.0f);
            x2_Timer.AnimationDuration = 10.0f;
            x2_Timer.SetProgress(0.0f);
            nMultiplier.Value = 1;
        }
    }

    public void DeactivateX2Timer()
    {
        x2_Timer.InstantSetProgress(0.0f);
        x2_Timer.AnimationDuration = 1.0f;
        nMultiplier.Value = 0;
    }

    public void ActionInTime(float progress) {
        if(progress == 0.0f && nMultiplier.Value == 1) { GameEventMessage.SendEvent("X2TimeOut"); }
    }
}
