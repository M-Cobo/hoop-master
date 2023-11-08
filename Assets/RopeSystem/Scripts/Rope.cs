using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

[RequireComponent(typeof(LineRenderer)), RequireComponent(typeof(EdgeCollider2D))]
public class Rope : MonoBehaviour
{
    #region NonSerialized Fields
    // Line/Rope Components
    private EdgeCollider2D edgeCollider;
    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private GameObject ropeSegmentsParent = null; 

    private Camera _Camera;

    private ContactFilter2D contactFilter;
    RaycastHit2D[] raycastHitBuffer = new RaycastHit2D[10];
    Collider2D[] colliderHitBuffer = new Collider2D[10];

    private bool pinStartPoint = true;
    #endregion

    #region Serialized Fields
    [SerializeField] private GameObject ropeSegment = null;
    [SerializeField] private int segmentLength = 35;
    [SerializeField] private float segmentsDistance = 0.25f;
    [SerializeField] private float ropeWidth = 0.1f;
    [SerializeField] private int iterations = 50;
    [SerializeField] private Vector2 forceGravity = new Vector2(0f, -1.5f);

    [SerializeField] private RopeType ropeType = RopeType.Mouse;
    [ShowIf("ropeType", RopeType.Bridge)]
    [SerializeField] private Transform startPoint = null, endPoint = null;
    [ShowIf("ropeType", RopeType.Pinned)]
    [SerializeField] private Transform pinnedPoint = null;

    [SerializeField] private LayerMask _layerMask = 1;
    #endregion

    public enum RopeType { Mouse, Bridge, Pinned }

    private void Awake() 
    {   
        this.edgeCollider = this.GetComponent<EdgeCollider2D>();
        this.lineRenderer = this.GetComponent<LineRenderer>();
        this._Camera = Camera.main;

        contactFilter = new ContactFilter2D 
        {
            layerMask = _layerMask,
            useTriggers = false,
        };
    }

    void Start()
    {
        Vector3 ropeStartPoint = Vector3.zero;
        switch (ropeType)
        {
            case RopeType.Mouse:
                ropeStartPoint = _Camera.ScreenToWorldPoint(Input.mousePosition);
            break;

            case RopeType.Bridge:
                CheckForPoints();
                ropeStartPoint = startPoint.position;
            break;

            case RopeType.Pinned:
                CheckForPoints();
                ropeStartPoint = pinnedPoint.position;
            break;
        }

        float lineWidth = this.ropeWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        ropeSegmentsParent = new GameObject("RopeSegmentsParent");
        for (int i = 0; i < segmentLength; i++)
        {
            GameObject segment = Instantiate(ropeSegment, ropeStartPoint, Quaternion.identity);
            segment.transform.localScale = new Vector2(lineWidth, lineWidth);
            segment.transform.SetParent(ropeSegmentsParent.transform);

            RopeSegment ropeSegmentComponent = segment.GetComponent<RopeSegment>();
            ropeSegmentComponent.PreviousPosition = ropeStartPoint;
            this.ropeSegments.Add(ropeSegmentComponent);
            ropeStartPoint.y -= segmentsDistance;
        }
    }

    private void OnDestroy() 
    {
        if(ropeSegmentsParent != null) {
            Destroy(ropeSegmentsParent);
        }
    }

    void Update()
    {
        if(ropeType == RopeType.Pinned)
        {
            if (Input.GetMouseButtonDown(1)) { pinStartPoint = !pinStartPoint; }

            if(Input.GetMouseButtonDown(0))
            {
                pinnedPoint.position = _Camera.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        this.DrawRope();
    }

    private void FixedUpdate()
    {
        this.Simulate();

        //CONSTRAINTS
        // Higher iteration results in stiffer ropes and stable simulation
        for (int i = 0; i < iterations; i++)
        {
            this.ApplyConstraint();

            // Playing around with adjusting collisions at intervals - still stable when iterations are skipped
            if (i % 2 == 1)
                AdjustCollisions();
        }
    }

    private void Simulate()
    {
        // SIMULATION
        for (int i = 1; i < this.segmentLength; i++)
        {
            RopeSegment currentSegment = this.ropeSegments[i];

            // Derive the velocity from previous frame
            Vector2 velocity = currentSegment.Position - currentSegment.PreviousPosition;
            currentSegment.PreviousPosition = currentSegment.Position;

            // Calculate new position
            Vector2 newPos = currentSegment.Position + velocity;
            newPos += forceGravity * Time.fixedDeltaTime;

            // Calculate direction of segment
            Vector3 direction = currentSegment.Position - newPos; 

            // Cast ray towards this position to check for a collision
            int result = -1;
            result = Physics2D.CircleCast(currentSegment.Position, currentSegment.Scale.x / 2f, -direction.normalized, contactFilter, raycastHitBuffer, direction.magnitude);

            if (result > 0)
            {
                for (int n = 0; n < result; n++)
                {                    
                    if (raycastHitBuffer[n].collider.gameObject.layer == 9)
                    {
                        Vector2 colliderCenter = new Vector2(raycastHitBuffer[n].collider.transform.position.x, raycastHitBuffer[n].collider.transform.position.y);
                        Vector2 collisionDirection = raycastHitBuffer[n].point - colliderCenter;
                        // adjusts the position based on a circle collider
                        Vector2 hitPos = colliderCenter + collisionDirection.normalized * (raycastHitBuffer[n].collider.transform.localScale.x / 2f + currentSegment.Scale.x / 2f);
                        newPos = hitPos;
                        break;              //Just assuming a single collision to simplify the model
                    }
                }
            }

            currentSegment.Position = newPos;
        }
    }

    private void ApplyConstraint()
    {
        RopeSegment firstSegment = this.ropeSegments[0];
        RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Count - 1];
        
        switch (ropeType)
        {
            #region Constrant to Mouse
            case RopeType.Mouse:
                firstSegment.Position = _Camera.ScreenToWorldPoint(Input.mousePosition);
            break;
            #endregion

            #region Bridge Constrant
            case RopeType.Bridge:
                //Constrant to First Point
                firstSegment.Position = this.startPoint.position;

                //Constrant to Second Point 
                endSegment.Position = this.endPoint.position;
            break;
            #endregion

            #region Pinned Point Constrant
            case RopeType.Pinned:
                if(pinStartPoint) {
                    //Constrant to First Point
                    firstSegment.Position = this.pinnedPoint.position;
                } else {
                    //Constrant to Second Point
                    endSegment.Position = this.pinnedPoint.position;
                }
            break;
            #endregion
        }


        for (int i = 0; i < this.segmentLength - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            // Get the current distance between rope nodes
            float dist = (firstSeg.Position - secondSeg.Position).magnitude;
            float error = Mathf.Abs(dist - this.segmentsDistance);
            Vector2 changeDir = Vector2.zero;

            // determine what direction we need to adjust our nodes
            if (dist > segmentsDistance)
            {
                changeDir = (firstSeg.Position - secondSeg.Position).normalized;
            } 
            else if (dist < segmentsDistance)
            {
                changeDir = (secondSeg.Position - firstSeg.Position).normalized;
            }

            // calculate the movement vector
            Vector2 movement = changeDir * error;

            // apply correction
            firstSeg.Position -= (movement * 0.5f);
            secondSeg.Position += (movement * 0.5f);
        }
    }

    private void AdjustCollisions()
    {
        // Loop rope nodes and check if currently colliding
        for (int i = 0; i < this.segmentLength - 1; i++)
        {
            RopeSegment segment = this.ropeSegments[i];

            int result = -1;
            result = Physics2D.OverlapCircleNonAlloc(segment.Position, segment.Scale.x / 2f, colliderHitBuffer);

            if (result > 0)
            {
                for (int n = 0; n < result; n++)
                {
                    if (colliderHitBuffer[n].gameObject.layer == 9/*!= 8*/)
                    {
                        // Adjust the rope node position to be outside collision
                        Vector3 colliderCenter = colliderHitBuffer[n].transform.position;
                        Vector3 collisionDirection = segment.transform.position - colliderCenter;

                        Vector3 hitPos = colliderCenter + collisionDirection.normalized * ((colliderHitBuffer[n].transform.localScale.x / 2f) + (segment.Scale.x / 2f));
                        segment.Position = hitPos;
                        break;
                    }
                }
            }
        }    
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[this.segmentLength];
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = this.ropeSegments[i].Position;
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);

        if(ropeType == RopeType.Mouse) { edgeCollider.points = ropePositions.ToVector2(); }
        else { edgeCollider.points = ropePositions.ToLocalVector2(transform); }
    }

    private void CheckForPoints()
    {
        switch (ropeType)
        {
            case RopeType.Bridge:
                if(startPoint == null)
                {
                    startPoint = new GameObject("StartPoint").transform;
                    startPoint.position = Vector2.left;
                    startPoint.SetParent(transform);
                }

                if(endPoint == null)
                {
                    endPoint = new GameObject("EndPoint").transform;
                    endPoint.position = Vector2.right;
                    endPoint.SetParent(transform);
                }
            break;

            case RopeType.Pinned:
                if(pinnedPoint == null)
                {
                    pinnedPoint = new GameObject("PinnedPoint").transform;
                    pinnedPoint.position = Vector2.zero;
                    pinnedPoint.SetParent(transform);
                }
            break;
        }
    }
}

public static class MyVector3Extension
{
    public static Vector2[] ToLocalVector2(this Vector3[] v3, Transform transform)
    {
        Vector3[] localV3 = new Vector3[v3.Length];
        for (int i = 0; i < v3.Length; i++)
        {
            localV3[i] = transform.InverseTransformPoint(v3[i]);
        }
        
        return localV3.ToVector2();
    }

    public static Vector2[] ToVector2(this Vector3[] v3)
    {
        return Array.ConvertAll<Vector3, Vector2>(v3, GetV2FromV3);
    }

    private static Vector2 GetV2FromV3(Vector3 v3)
    {
        return new Vector2(v3.x, v3.y);
    }
}
