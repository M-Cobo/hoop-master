using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Level", menuName = "Custom/Levels/New Level")]
[InlineEditor]
public class Level : SerializedScriptableObject
{
    private enum Matrix { Map_3x3, Map_4x4 }
    [HideLabel, EnumToggleButtons]
    [SerializeField] private Matrix matrix = Matrix.Map_3x3;

    [ShowIf("matrix", Matrix.Map_3x3)]
    [SerializeField, TabGroup("Position"), TableMatrix(SquareCells = true), AssetsOnly]
    private GameObject[,] LevelMap3x3 = new GameObject[3, 3];

    [ShowIf("matrix", Matrix.Map_3x3)]
    [SerializeField, TabGroup("Rotation"), TableMatrix(SquareCells = true)]
    private float[,] InitRot3x3 = new float[3, 3];

    [ShowIf("matrix", Matrix.Map_3x3)]
    [SerializeField, TabGroup("Animation"), TableMatrix(SquareCells = true)]
    private TweenAnimation[,] Animation3x3 = new TweenAnimation[3, 3];


    [HideIf("matrix", Matrix.Map_3x3)]
    [SerializeField, TabGroup("Position"), TableMatrix(SquareCells = true), AssetsOnly]
    private GameObject[,] LevelMap4x4 = new GameObject[4, 4];

    [HideIf("matrix", Matrix.Map_3x3)]
    [SerializeField, TabGroup("Rotation"), TableMatrix(SquareCells = true)]
    private float[,] InitRot4x4 = new float[4, 4];

    [HideIf("matrix", Matrix.Map_3x3)]
    [SerializeField, TabGroup("Animation"), TableMatrix(SquareCells = true)]
    private TweenAnimation[,] Animation4x4 = new TweenAnimation[4, 4];

    private Vector2[,] SpawnCoords3x3 = new Vector2[3, 3]
    {
        { new Vector2(-3, 7) , new Vector2(-3, 4) , new Vector2(-3, 1)}, 
        { new Vector2(0 , 7) , new Vector2(0 , 4) , new Vector2(0 , 1)},
        { new Vector2(3 , 7) , new Vector2(3 , 4) , new Vector2(3 , 1)},
    };

    private Vector2[,] SpawnCoords4x4 = new Vector2[4, 4]
    {
        { new Vector2(-4    , 7.5f) , new Vector2(-4    , 4.75f) , new Vector2(-4    , 2) , new Vector2(-4    , -0.75f)}, 
        { new Vector2(-1.25f, 7.5f) , new Vector2(-1.25f, 4.75f) , new Vector2(-1.25f, 2) , new Vector2(-1.25f, -0.75f)},
        { new Vector2(1.25f , 7.5f) , new Vector2(1.25f , 4.75f) , new Vector2(1.25f , 2) , new Vector2(1.25f , -0.75f)},
        { new Vector2(4     , 7.5f) , new Vector2(4     , 4.75f) , new Vector2(4     , 2) , new Vector2(4     , -0.75f)},
    };

    [BoxGroup("Animations", false)]
    [SerializeField] private float delayToDestroy = 1;
    [BoxGroup("Animations", false)]
    [SerializeField] private TweenAnimation LevelIn = null;
    [BoxGroup("Animations", false)]
    [SerializeField] private TweenAnimation LevelOut = null;

    [Button(ButtonSizes.Medium)]
    public IEnumerator Spawn(float t)
    {
        LevelExists();

        yield return new WaitForSeconds(t);

        Transform p_Level = new GameObject("Level").transform;

        GameObject[,] LevelMap;
        Vector2[,] SpawnCoords;
        float[,] InitRot;
        TweenAnimation[,] tweenAnimation;

        Vector2 matrixSize;

        if(matrix == Matrix.Map_3x3) {
            LevelMap = LevelMap3x3;
            SpawnCoords = SpawnCoords3x3;
            InitRot = InitRot3x3;
            matrixSize = new Vector2(3, 3);
            tweenAnimation = Animation3x3;
        } else {
            LevelMap = LevelMap4x4;
            SpawnCoords = SpawnCoords4x4;
            InitRot = InitRot4x4;
            matrixSize = new Vector2(4, 4);
            tweenAnimation = Animation4x4;
        }
        
        // Loop through Rows 
        for (int i = 0; i < matrixSize.x; i++)
        {
            // Loop through Columns 
            for (int j = 0; j < matrixSize.y; j++)
            {
                if(LevelMap[i, j] != null)
                {
                    GameObject _Obstacle = Instantiate(
                        LevelMap[i, j],
                        SpawnCoords[i, j],
                        Quaternion.Euler(new Vector3(0, 0, InitRot[i, j]))
                    );

                    _Obstacle.transform.SetParent(p_Level);

                    LevelIn.Play(_TransformTarget: _Obstacle.transform);
                    
                    if(tweenAnimation[i, j] != null) {
                        if(_Obstacle.transform.GetChild(0).tag == "Animate") {
                            tweenAnimation[i, j].Play(_TransformTarget: _Obstacle.transform.GetChild(0));
                        } else {
                            Debug.LogWarning(string.Format("Could not find an object with tag 'Animate' in: {0} obstacle", _Obstacle.name));
                        }
                    }
                }
            }
        }
    }

    private void LevelExists()
    {
        GameObject level = GameObject.Find("Level");
        if(level != null) { 
            foreach(Transform obstacle in level.transform)
            {
                LevelOut.Play(_TransformTarget: obstacle);
            }

            Destroy(level, delayToDestroy); 
        }
    }
}
