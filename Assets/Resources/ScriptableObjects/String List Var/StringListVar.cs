using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "StringList", menuName = "Custom/Variables/New StringList")]
[InlineEditor]
public class StringListVar : ScriptableObject
{
    public List<string> Value;
}
