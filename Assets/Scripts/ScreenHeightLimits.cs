using UnityEngine;

public class ScreenHeightLimits : MonoBehaviour 
{
	[SerializeField] private EdgeCollider2D[] edgesVertical = null;
	[SerializeField] private EdgeCollider2D[] edgesHorizontal = null;

	private Vector2[] newVPoints = new Vector2[] { new Vector2(0, 0),  new Vector2(0, 0) };
	private Vector2[] newHPoints = new Vector2[] { new Vector2(0, 0),  new Vector2(0, 0) };

	private void OnEnable() 
	{
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float cameraHeight = Camera.main.orthographicSize * 2;
		Vector2 bounds = new Vector2(cameraHeight * screenAspect, cameraHeight);

		newVPoints[0] = new Vector2(0, bounds.y / 2);
		newVPoints[1] = new Vector2(0, -(bounds.y / 2));

		foreach (var edgeCollider in edgesVertical)
		{
			edgeCollider.points = newVPoints;
		}
		
		newHPoints[0] = new Vector2((bounds.x / 2), 0);
		newHPoints[1] = new Vector2(-(bounds.x / 2), 0);

		foreach (var edgeCollider in edgesHorizontal)
		{
			edgeCollider.points = newHPoints;
		}
	}
}