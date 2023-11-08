using System;
using UnityEngine;

public class CircleControl : MonoBehaviour 
{
    [SerializeField] private Hoop[] hoops = new Hoop[2];
    private Hoop hoopSelected = null;
    private Hoop hoopInverted = null;

    private CircleCollider2D circleCollider;
    private Camera _Camera;

    private Vector2 offset = Vector2.zero;

    [Serializable]
    public class Hoop {
        [SerializeField] private Transform hoopObj = null;
        public Vector2 ObjPosition { set { hoopObj.position = value; } }

        [SerializeField] private Transform hoopPos = null;
        public Vector2 FinalWorldPos { get { return hoopPos.position; } }
        public Vector2 LocalPosition { get { return hoopPos.localPosition; } set { hoopPos.localPosition = value; } }
        
        [SerializeField] private ParticleSystem smokeEffect = null;
        public ParticleSystem SmokeEffect { get { return smokeEffect; } }
        [SerializeField] private ParticleSystem fireEffect = null;
        public ParticleSystem FireEffect { get { return fireEffect; } }

        [SerializeField] private TrailRenderer[] hoopTrails = new TrailRenderer[2];
        public TrailRenderer EdgeTrail_1 { get { return hoopTrails[0]; } }
        public TrailRenderer EdgeTrail_2 { get { return hoopTrails[1]; } }
    }

    private void Awake() 
    {
        _Camera = Camera.main;
        hoopSelected = hoops[0];
        hoopInverted = hoops[1];
        circleCollider = this.GetComponent<CircleCollider2D>();
    }

    public void SetEffects(IntVariable perfectDunks) {
        switch (perfectDunks.Value)
        {
            case 1:
                foreach (Hoop hoop in hoops) {
                    hoop.SmokeEffect.Play();
                    hoop.EdgeTrail_1.enabled = true;
                    hoop.EdgeTrail_2.enabled = true;
                }
            break;

            case 2:
                foreach (Hoop hoop in hoops) {
                    hoop.FireEffect.Play();
                    hoop.SmokeEffect.Stop();
                }
            break;

            default:
                foreach (Hoop hoop in hoops) {
                    hoop.FireEffect.Stop();
                    hoop.SmokeEffect.Stop();
                    hoop.EdgeTrail_1.enabled = false;
                    hoop.EdgeTrail_2.enabled = false;
                }
            break;
        }
    }

    void Update ()
    {
        if(Input.GetMouseButtonDown(0))
        {

            Vector2 initMouseWorldPos = _Camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 initMouseLocalPos = transform.InverseTransformPoint(initMouseWorldPos);

            hoopSelected = NearestHoop(initMouseLocalPos, hoops);
            hoopInverted = hoopSelected == hoops[0] ? hoops[1] : hoops[0];

            offset = (hoopSelected.LocalPosition) - (initMouseLocalPos); // Get distance between the initial touch and the hoop position
        }

        if (Input.GetMouseButton(0))
        {
            // Get touch world position & turn it to local position
            Vector2 mouseWorldPos = _Camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseLocalPos = transform.InverseTransformPoint(mouseWorldPos);

            Vector2 circleCenter = Vector2.zero; // Center of the circle movement zone
            // Create a new position with the distance between the center of the circle and the touch position
            Vector2 newPos = (mouseLocalPos - circleCenter); 
            newPos = (newPos + offset); // Add offset to the new position of the hoop 
            newPos = Vector3.ClampMagnitude(newPos, circleCollider.radius); // Set new position inside the limits of a circle shape

            float circleBounds = (circleCollider.radius / 2); // Reduce radius at half
            float yMinLimit = -circleBounds; // Get min "Y" local position
            float yMaxLimit = circleBounds; // Get max "Y" local position
            newPos.y = Mathf.Clamp(newPos.y, yMinLimit, yMaxLimit); // Set "Y" coords of new position inside the "Y" coords limits
            
            Vector2 finalPos = (circleCenter + newPos);

            hoopSelected.LocalPosition = (finalPos);
            hoopInverted.LocalPosition = (-finalPos);
        }
    }

    private void LateUpdate() 
    {
        hoopSelected.ObjPosition = hoopSelected.FinalWorldPos;
        hoopInverted.ObjPosition = hoopInverted.FinalWorldPos;
    }

    Hoop NearestHoop(Vector2 touch, Hoop[] hoops)
    {
        float firstHoopDistance = Vector2.Distance(touch, hoops[0].LocalPosition);
        float secondHoopDistance = Vector2.Distance(touch, hoops[1].LocalPosition);
        float nearest = Mathf.Min(firstHoopDistance, secondHoopDistance);
        
        if(firstHoopDistance == nearest) {
            return hoops[0];
        }
        else if (secondHoopDistance == nearest) {
            return hoops[1];
        }
        else {
            return hoops[0];
        }
    }
}
