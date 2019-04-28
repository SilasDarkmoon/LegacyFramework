using System.Collections.Generic;
using UnityEngine;

namespace BezierSolution
{
    [ExecuteInEditMode]
    public class BezierSpline
    {
        private static Material gizmoMaterial;

        private Color gizmoColor = Color.white;
        private float gizmoStep = 0.05f;

        private List<BezierPoint> endPoints = new List<BezierPoint>();

        public bool loop = false;
        public bool drawGizmos = false;

        public int Count { get { return endPoints.Count; } }
        public float Length { get { return GetLengthApproximately(0f, 1f); } }

        public BezierPoint this[int index]
        {
            get
            {
                if (index < Count)
                    return endPoints[index];

                Debug.LogError("Bezier index " + index + " is out of range: " + Count);
                return null;
            }
        }

        public void RemovePointAt(int index)
        {
            if (endPoints.Count <= 2)
            {
                Debug.LogError("Can't remove point: spline must consist of at least two points!");
                return;
            }

            if (index < 0 || index >= endPoints.Count)
            {
                Debug.LogError("Index " + index + " is out of range: [0," + endPoints.Count + ")");
                return;
            }

            BezierPoint point = endPoints[index];
            endPoints.RemoveAt(index);
        }

        public int IndexOf(BezierPoint point)
        {
            return endPoints.IndexOf(point);
        }

        public void DrawGizmos(Color color, int smoothness = 4)
        {
            drawGizmos = true;
            gizmoColor = color;
            gizmoStep = 1f / (endPoints.Count * Mathf.Clamp(smoothness, 1, 30));
        }

        public void HideGizmos()
        {
            drawGizmos = false;
        }

        public Vector3 GetPoint(float normalizedT)
        {
            if (normalizedT <= 0f)
                return endPoints[0].position;
            else if (normalizedT >= 1f)
            {
                if (loop)
                    return endPoints[0].position;

                return endPoints[endPoints.Count - 1].position;
            }

            float t = normalizedT * (loop ? endPoints.Count : (endPoints.Count - 1));

            BezierPoint startPoint, endPoint;

            int startIndex = (int)t;
            int endIndex = startIndex + 1;

            if (endIndex == endPoints.Count)
                endIndex = 0;

            startPoint = endPoints[startIndex];
            endPoint = endPoints[endIndex];

            float localT = t - startIndex;
            float oneMinusLocalT = 1f - localT;

            return oneMinusLocalT * oneMinusLocalT * oneMinusLocalT * startPoint.position +
                   3f * oneMinusLocalT * oneMinusLocalT * localT * startPoint.followingControlPointPosition +
                   3f * oneMinusLocalT * localT * localT * endPoint.precedingControlPointPosition +
                   localT * localT * localT * endPoint.position;
        }

        public Vector3 GetTangent(float normalizedT)
        {
            if (normalizedT <= 0f)
                return 3f * (endPoints[0].followingControlPointPosition - endPoints[0].position);
            else if (normalizedT >= 1f)
            {
                if (loop)
                    return 3f * (endPoints[0].position - endPoints[0].precedingControlPointPosition);
                else
                {
                    int index = endPoints.Count - 1;
                    return 3f * (endPoints[index].position - endPoints[index].precedingControlPointPosition);
                }
            }

            float t = normalizedT * (loop ? endPoints.Count : (endPoints.Count - 1));

            BezierPoint startPoint, endPoint;

            int startIndex = (int)t;
            int endIndex = startIndex + 1;

            if (endIndex == endPoints.Count)
                endIndex = 0;

            startPoint = endPoints[startIndex];
            endPoint = endPoints[endIndex];

            float localT = t - startIndex;
            float oneMinusLocalT = 1f - localT;

            return 3f * oneMinusLocalT * oneMinusLocalT * (startPoint.followingControlPointPosition - startPoint.position) +
                   6f * oneMinusLocalT * localT * (endPoint.precedingControlPointPosition - startPoint.followingControlPointPosition) +
                   3f * localT * localT * (endPoint.position - endPoint.precedingControlPointPosition);
        }

        public float GetLengthApproximately(float startNormalizedT, float endNormalizedT, float accuracy = 50f)
        {
            if (endNormalizedT < startNormalizedT)
            {
                float temp = startNormalizedT;
                startNormalizedT = endNormalizedT;
                endNormalizedT = temp;
            }

            if (startNormalizedT < 0f)
                startNormalizedT = 0f;

            if (endNormalizedT > 1f)
                endNormalizedT = 1f;

            float step = AccuracyToStepSize(accuracy) * (endNormalizedT - startNormalizedT);

            float length = 0f;
            Vector3 lastPoint = GetPoint(startNormalizedT);
            for (float i = startNormalizedT + step; i < endNormalizedT; i += step)
            {
                Vector3 thisPoint = GetPoint(i);
                length += Vector3.Distance(thisPoint, lastPoint);
                lastPoint = thisPoint;
            }

            length += Vector3.Distance(lastPoint, GetPoint(endNormalizedT));

            return length;
        }

        public Vector3 FindNearestPointTo(Vector3 worldPos, float accuracy = 100f)
        {
            float normalizedT;
            return FindNearestPointTo(worldPos, out normalizedT, accuracy);
        }

        public Vector3 FindNearestPointTo(Vector3 worldPos, out float normalizedT, float accuracy = 100f)
        {
            Vector3 result = Vector3.zero;
            normalizedT = -1f;

            float step = AccuracyToStepSize(accuracy);

            float minDistance = Mathf.Infinity;
            for (float i = 0f; i < 1f; i += step)
            {
                Vector3 thisPoint = GetPoint(i);
                float thisDistance = (worldPos - thisPoint).sqrMagnitude;
                if (thisDistance < minDistance)
                {
                    minDistance = thisDistance;
                    result = thisPoint;
                    normalizedT = i;
                }
            }

            return result;
        }

        // Obsolete method, changed with a faster and more accurate variant
        //public Vector3 MoveAlongSpline( ref float normalizedT, float deltaMovement, 
        //								bool increasedAccuracy = false, int maximumNumberOfChecks = 20, float maximumError = 0.001f )
        //{
        //	// Maybe that one is a better approach? https://www.geometrictools.com/Documentation/MovingAlongCurveSpecifiedSpeed.pdf

        //	if( Mathf.Approximately( deltaMovement, 0f ) )
        //		return GetPoint( normalizedT );

        //	if( maximumNumberOfChecks < 3 )
        //		maximumNumberOfChecks = 3;

        //	normalizedT = Mathf.Clamp01( normalizedT );

        //	Vector3 point = GetPoint( normalizedT );
        //	float deltaMovementSqr = deltaMovement * deltaMovement;
        //	bool isForwardDir = deltaMovement > 0;

        //	float bestNormalizedT;
        //	float maxNormalizedT, minNormalizedT;

        //	float error;
        //	Vector3 result;

        //	if( isForwardDir )
        //	{
        //		bestNormalizedT = ( 1f + normalizedT ) * 0.5f;

        //		maxNormalizedT = 1f;
        //		minNormalizedT = normalizedT;
        //	}
        //	else
        //	{
        //		bestNormalizedT = normalizedT * 0.5f;

        //		maxNormalizedT = normalizedT;
        //		minNormalizedT = 0f;
        //	}

        //	result = GetPoint( bestNormalizedT );

        //	if( !increasedAccuracy )
        //	{
        //		error = ( result - point ).sqrMagnitude - deltaMovementSqr;
        //	}
        //	else
        //	{
        //		float distance = GetLengthApproximately( normalizedT, bestNormalizedT, 10f );
        //		error = distance * distance - deltaMovementSqr;
        //	}

        //	if( !isForwardDir )
        //		error = -error;

        //	if( Mathf.Abs( error ) > maximumError )
        //	{
        //		for( int i = 0; i < maximumNumberOfChecks; i++ )
        //		{
        //			if( error > 0 )
        //			{
        //				maxNormalizedT = bestNormalizedT;
        //				bestNormalizedT = ( bestNormalizedT + minNormalizedT ) * 0.5f;
        //			}
        //			else
        //			{
        //				minNormalizedT = bestNormalizedT;
        //				bestNormalizedT = ( bestNormalizedT + maxNormalizedT ) * 0.5f;
        //			}

        //			result = GetPoint( bestNormalizedT );

        //			if( !increasedAccuracy )
        //			{
        //				error = ( result - point ).sqrMagnitude - deltaMovementSqr;
        //			}
        //			else
        //			{
        //				float distance = GetLengthApproximately( normalizedT, bestNormalizedT, 10f );
        //				error = distance * distance - deltaMovementSqr;
        //			}

        //			if( !isForwardDir )
        //				error = -error;

        //			if( Mathf.Abs( error ) <= maximumError )
        //			{
        //				break;
        //			}
        //		}
        //	}

        //	normalizedT = bestNormalizedT;
        //	return result;
        //}

        public Vector3 MoveAlongSpline(ref float normalizedT, float deltaMovement, int accuracy = 3)
        {
            // Credit: https://gamedev.stackexchange.com/a/27138

            float _1OverCount = 1f / endPoints.Count;
            for (int i = 0; i < accuracy; i++)
                normalizedT += deltaMovement * _1OverCount / (accuracy * GetTangent(normalizedT).magnitude);

            return GetPoint(normalizedT);
        }

        public void ConstructLinearPath(List<Vector3> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                var endPoint = new BezierPoint();
                Debug.Log(points[i]);
                endPoint.position = points[i];
                endPoints.Add(endPoint);
            }
            for (int i = 0; i < endPoints.Count; i++)
            {
                endPoints[i].handleMode = BezierPoint.HandleMode.Free;

                if (i < endPoints.Count - 1)
                {
                    Vector3 midPoint = (endPoints[i].position + endPoints[i + 1].position) * 0.5f;
                    endPoints[i].followingControlPointPosition = midPoint;
                    endPoints[i + 1].precedingControlPointPosition = midPoint;
                }
                else
                {
                    Vector3 midPoint = (endPoints[i].position + endPoints[0].position) * 0.5f;
                    endPoints[i].followingControlPointPosition = midPoint;
                    endPoints[0].precedingControlPointPosition = midPoint;
                }
            }
        }

        private static Vector3[] GetFirstControlPoints(Vector3[] rhs)
        {
            // Credit: http://www.codeproject.com/Articles/31859/Draw-a-Smooth-Curve-through-a-Set-of-2D-Points-wit

            int n = rhs.Length;
            Vector3[] x = new Vector3[n]; // Solution vector.
            float[] tmp = new float[n]; // Temp workspace.

            float b = 2f;
            x[0] = rhs[0] / b;
            for (int i = 1; i < n; i++) // Decomposition and forward substitution.
            {
                float val = 1f / b;
                tmp[i] = val;
                b = (i < n - 1 ? 4f : 3.5f) - val;
                x[i] = (rhs[i] - x[i - 1]) / b;
            }

            for (int i = 1; i < n; i++)
            {
                x[n - i - 1] -= tmp[n - i] * x[n - i]; // Backsubstitution.
            }

            return x;
        }

        public void AutoConstructSpline2(List<Vector3> points)
        {
            // Credit: http://stackoverflow.com/questions/3526940/how-to-create-a-cubic-bezier-curve-when-given-n-points-in-3d
            for (int i = 0; i < points.Count; i++)
            {
                var endPoint = new BezierPoint();
                Debug.Log(points[i]);
                endPoint.position = points[i];
                endPoints.Add(endPoint);
            }
            for (int i = 0; i < endPoints.Count; i++)
            {
                Vector3 pMinus1, p1, p2;

                if (i == 0)
                {
                    if (loop)
                        pMinus1 = endPoints[endPoints.Count - 1].position;
                    else
                        pMinus1 = endPoints[0].position;
                }
                else
                {
                    pMinus1 = endPoints[i - 1].position;
                }

                if (loop)
                {
                    p1 = endPoints[(i + 1) % endPoints.Count].position;
                    p2 = endPoints[(i + 2) % endPoints.Count].position;
                }
                else
                {
                    if (i < endPoints.Count - 2)
                    {
                        p1 = endPoints[i + 1].position;
                        p2 = endPoints[i + 2].position;
                    }
                    else if (i == endPoints.Count - 2)
                    {
                        p1 = endPoints[i + 1].position;
                        p2 = endPoints[i + 1].position;
                    }
                    else
                    {
                        p1 = endPoints[i].position;
                        p2 = endPoints[i].position;
                    }
                }

                endPoints[i].followingControlPointPosition = endPoints[i].position + (p1 - pMinus1) / 6f;
                endPoints[i].handleMode = BezierPoint.HandleMode.Mirrored;

                if (i < endPoints.Count - 1)
                    endPoints[i + 1].precedingControlPointPosition = p1 - (p2 - endPoints[i].position) / 6f;
                else if (loop)
                    endPoints[0].precedingControlPointPosition = p1 - (p2 - endPoints[i].position) / 6f;
            }
        }

        /*public void AutoConstructSpline3()
		{
			// Todo? http://www.math.ucla.edu/~baker/149.1.02w/handouts/dd_splines.pdf
		}*/

        private float AccuracyToStepSize(float accuracy)
        {
            if (accuracy <= 0f)
                return 0.2f;

            return Mathf.Clamp(1f / accuracy, 0.001f, 0.2f);
        }

        // Renders the spline gizmo during gameplay
        // Credit: https://docs.unity3d.com/ScriptReference/GL.html
        private void OnRenderObject()
        {
            if (!drawGizmos || endPoints.Count < 2)
                return;

            if (!gizmoMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                gizmoMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                gizmoMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                gizmoMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                gizmoMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                gizmoMaterial.SetInt("_ZWrite", 0);
            }

            gizmoMaterial.SetPass(0);

            GL.Begin(GL.LINES);
            GL.Color(gizmoColor);

            Vector3 lastPos = endPoints[0].position;

            for (float i = gizmoStep; i < 1f; i += gizmoStep)
            {
                GL.Vertex3(lastPos.x, lastPos.y, lastPos.z);
                lastPos = GetPoint(i);
                GL.Vertex3(lastPos.x, lastPos.y, lastPos.z);
            }

            GL.Vertex3(lastPos.x, lastPos.y, lastPos.z);
            lastPos = GetPoint(1f);
            GL.Vertex3(lastPos.x, lastPos.y, lastPos.z);

            GL.End();
        }
    }
}