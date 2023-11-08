using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Float", menuName = "Custom/Variables/New Float")]
[InlineEditor]
public class FloatVariable : ScriptableObject, ISerializationCallbackReceiver
{
    public float Value = 0;

    public void OnAfterDeserialize() { SceneManager.activeSceneChanged += OnSceneChanged; } 
    public void OnBeforeSerialize() { }
         
    void OnSceneChanged(Scene current, Scene next)
    {
        Value = 0;
    }
}
