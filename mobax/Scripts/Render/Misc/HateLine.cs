using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HateLine : MonoBehaviour
{
	public float BeginOffset, EndOffset;
	public LineRenderer Line;
	public Vector2 SecondPointOffset = new Vector2(1, 2);
	public int LineCount = 20;
	public GameObject PointObject;
	[Range(0, 1)]
	public float FadeFactor;

	// Quadratic Bezier
	public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		t = Mathf.Clamp01(t);
		float oneMinusT = 1f - t;
		return
			oneMinusT * oneMinusT * p0 +
			2f * oneMinusT * t * p1 +
			t * t * p2;
	}

	float Length = 0;
	Vector3 MidPoint;
	Material LineMaterial;
	Transform BeginTransform;
	Transform EndTransform;
	List<Vector3> LinePoints = new List<Vector3>();
	public void DrawLine (GameObject beginObject, GameObject endObject, float duration = 0.5f, float stay = 0.5f, float backwardDuration = 0.5f)
    {
		BeginTransform = beginObject.transform;
		EndTransform = endObject.transform;
		LineMaterial = Line.material;
		Line.gameObject.SetActive(true);
		PointObject.SetActive(false);
		ConstructLine();

		// animate line effect
		StopAllCoroutines();
		StartCoroutine(AnimLineCoroutine(duration, stay, backwardDuration));
	}
	void ConstructLine ()
    {
		if (BeginTransform == null || EndTransform == null) {
			Reset();
			return;
		}

		// points
		Length = 0;
		LinePoints.Clear();
		var segment = 1f / LineCount;
		var beginPoint = BeginTransform.position + Vector3.up * BeginOffset;
		var endPoint = EndTransform.position + Vector3.up * EndOffset;
		var direction = endPoint - beginPoint;
		MidPoint = (direction.magnitude > SecondPointOffset.x * 2 ? beginPoint + direction.normalized * SecondPointOffset.x : beginPoint + direction * 0.5f) + Vector3.up * SecondPointOffset.y;
		for (int i = 0; i < LineCount; i++) {
			var point = GetPoint(beginPoint, MidPoint, endPoint, segment * i);
			LinePoints.Add(point);
			if (i > 0) {
				Length += Vector3.Magnitude (LinePoints[i] - LinePoints[i - 1]);

			}
		}

		// construct line
		Line.positionCount = LineCount;
		for (var i = 0; i < LinePoints.Count; i++) {
			Line.SetPosition(i, LinePoints[i]);
		}
	}

	IEnumerator AnimLineCoroutine (float duration, float stay, float backward)
    {
		float elapsed = 0;
		// point to target
		LineMaterial.SetFloat("_Inverse", 0);
		LineMaterial.SetFloat("_Fade", 0);
		PointObject.SetActive(true);
		while (elapsed < duration) {
			var progress = Mathf.Min (elapsed / duration, 1);
			var total = Length * progress;
			LineMaterial.SetFloat("_Progress", progress);
			// fx point 
			var pointPos = LinePoints[0];
			for (var i = 1; i < LinePoints.Count; i++) {
				var l = Vector3.Magnitude(LinePoints[i] - LinePoints[i - 1]);
				total -= l;
				if (total < 0) {
					total += l;
					pointPos = LinePoints[i - 1] + (LinePoints[i] - LinePoints[i - 1]) * total;
					break;
                }
            }
			PointObject.transform.position = pointPos;

			yield return new WaitForEndOfFrame();
			ConstructLine();
			elapsed += Time.deltaTime;
        }
		LineMaterial.SetFloat("_Progress", 1);
		PointObject.SetActive(false);

		elapsed = 0;
		// stay
		while (elapsed < stay) {
			yield return new WaitForEndOfFrame();
			ConstructLine();
			elapsed += Time.deltaTime;
		}

		elapsed = 0;
		var fadeDuration = duration * FadeFactor;
		// backward  
		LineMaterial.SetFloat("_Inverse", 1);
		LineMaterial.SetFloat("_Fade", 0);
		while (elapsed < backward) {
			var fadeProgress = Mathf.Min(elapsed / fadeDuration, 1);
			LineMaterial.SetFloat("_Fade", fadeProgress);

			var progress = Mathf.Min(elapsed / backward, 1);
			LineMaterial.SetFloat("_Progress", 1 - progress);

			yield return new WaitForEndOfFrame();
			ConstructLine();
			elapsed += Time.deltaTime;
		}
		LineMaterial.SetFloat("_Progress", 0);
	}
    public void Reset()
    {
		StopAllCoroutines();
		Line.gameObject.SetActive(false);
		PointObject.SetActive(false);
	}
    private void OnDestroy()
    {
		StopAllCoroutines();
    }

    [BoxGroup ("Test")]
	public GameObject TestStartPoint;
	[BoxGroup("Test")]
	public GameObject TestEndPoint;
	[BoxGroup("Test")]
	public float TestDuration;
	[BoxGroup("Test")]
	public float TestStay;
	[BoxGroup("Test")]
	public float TestBackward;
	[BoxGroup("Test")]
	[Button ("Test")]
	void Test ()
    {
		if (TestStartPoint == null)
			TestStartPoint = GameObject.Find("TestStartPoint");
		if (TestEndPoint == null)
			TestEndPoint = GameObject.Find("TestEndPoint");

		if (TestStartPoint == null || TestEndPoint == null)
			return;
		StartCoroutine(TestCoroutine());
    }
	IEnumerator TestCoroutine ()
    {
		BeginTransform = TestStartPoint.transform;
		EndTransform = TestEndPoint.transform;
		LineMaterial = Line.material;
		ConstructLine();

		// animate line effect
		yield return StartCoroutine(AnimLineCoroutine(TestDuration, TestStay, TestBackward));
	}
}
