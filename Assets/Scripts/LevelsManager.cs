using UnityEngine;
using Doozy.Engine;
using System.Collections;
using Sirenix.OdinInspector;

public class LevelsManager : MonoBehaviour
{
    [TitleGroup("Level Settings", Alignment = TitleAlignments.Centered)]

    [BoxGroup("Level Settings/Phases Quantity by Level", CenterLabel = true)]
    [LabelText("Min"), LabelWidth(25), PropertyRange(2, "_MaxPhases"), HorizontalGroup("Level Settings/Phases Quantity by Level/SplitMinMax")]
    [SerializeField] private int _MinPhases = 2;

    [LabelText("Max"), LabelWidth(25), PropertyRange("_MinPhases", 4), HorizontalGroup("Level Settings/Phases Quantity by Level/SplitMinMax")]
    [SerializeField] private int _MaxPhases = 4;

    [BoxGroup("Level Settings/Points Quantity by Phase", CenterLabel = true)]
    [LabelText("Min"), LabelWidth(25), PropertyRange(3, "_MaxPoints"), HorizontalGroup("Level Settings/Points Quantity by Phase/SplitMinMax")]
    [SerializeField] private int _MinPoints = 3;

    [LabelText("Max"), LabelWidth(25), PropertyRange("_MinPoints", 5), HorizontalGroup("Level Settings/Points Quantity by Phase/SplitMinMax")]
    [SerializeField] private int _MaxPoints = 5;

    [SerializeField] private Color[] backgroundColors = new Color[0];

    [TabGroup("Easy Levels"), HideLabel]
    [SerializeField] private Level[] l_Easy = new Level[0];

    [TabGroup("Medium Levels"), HideLabel]
    [SerializeField] private Level[] l_Medium = new Level[0];

    [TabGroup("Hard Levels"), HideLabel]
    [SerializeField] private Level[] l_Hard = new Level[0];

    [TitleGroup("Level Setup", Alignment = TitleAlignments.Centered)]

    [LabelText("Level Phases")]
    [SerializeField] private IntVariable v_Phases = null;  

    [LabelText("Phase Points")]
    [SerializeField] private IntVariable v_Points = null;  

    [TitleGroup("Dynamic Data", Alignment = TitleAlignments.Centered)]

    [LabelText("Current Phase")]
    [SerializeField] private IntVariable c_Phase = null;  

    [LabelText("Current Points")]
    [SerializeField] private IntVariable c_Points = null;

    [EnumToggleButtons]
    private LevelDifficulties levelDifficulty = LevelDifficulties.Medium;
    enum LevelDifficulties { Easy, Medium, Hard }  

    private void Start() 
    {
        CreateLevel();
    }

    private void CreateLevel()
    {
        Camera.main.backgroundColor = backgroundColors[Random.Range(0, backgroundColors.Length)];
        
        v_Phases.Value = Random.Range(_MinPhases, (_MaxPhases + 1));
        v_Points.Value = Random.Range(_MinPoints, (_MaxPoints + 1));

        SpawnNewLevel(0.0f);
    }

    public void NextPhase()
    {
        c_Phase.Value += 1;
        if(c_Phase.Value >= v_Phases.Value) { 
            GameEventMessage.SendEvent("GameOver");
            GameEventMessage.SendEvent("NextPhase");
            return;
        }
        
        StartCoroutine(SetNewPhase());
    }

    private IEnumerator SetNewPhase()
    {
        int newPhasePoints = (v_Points.Value + Random.Range(3, 6));
        v_Points.Value = Mathf.Clamp(newPhasePoints, _MinPoints, 14);
        c_Points.Value = 0;

        SpawnNewLevel(3.0f);
        GameEventMessage.SendEvent("NextPhase");

        yield return new WaitForSeconds(3.0f);

        GameManager.Instance.SetGameState(GameState.Playing);
    }

    private void SpawnNewLevel(float timeToSpawn)
    {
        levelDifficulty = (LevelDifficulties)Mathf.Clamp(c_Phase.Value, 0, 3);
        switch (levelDifficulty)
        {
            case LevelDifficulties.Easy:
                StartCoroutine(l_Easy[Random.Range(0, l_Easy.Length)].Spawn(timeToSpawn));
            break;
            case LevelDifficulties.Medium:
                StartCoroutine(l_Medium[Random.Range(0, l_Medium.Length)].Spawn(timeToSpawn));
            break;
            case LevelDifficulties.Hard:
                StartCoroutine(l_Hard[Random.Range(0, l_Hard.Length)].Spawn(timeToSpawn));
            break;
        }
    }
}
