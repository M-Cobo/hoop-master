using UnityEngine;

public class RopeSegment : MonoBehaviour 
{
    public Vector2 PreviousPosition;
    public Vector2 Position
    {
        get
        {
            return transform.position;
        }
        set
        {
            transform.position = value;
        }
    }

    public Vector3 Scale
    {
        get
        {
            return transform.localScale;
        }
        set
        {
            transform.localScale = value;
        }
    }
}
