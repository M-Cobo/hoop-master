using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Int", menuName = "Custom/Variables/New Int")]
[InlineEditor]
public class IntVariable : ScriptableObject, ISerializationCallbackReceiver
{
    public int Value = 0;

    public void OnAfterDeserialize() { SceneManager.activeSceneChanged += OnSceneChanged; } 
    public void OnBeforeSerialize() { }
         
    void OnSceneChanged(Scene current, Scene next)
    {
        Value = 0;
    }
}
