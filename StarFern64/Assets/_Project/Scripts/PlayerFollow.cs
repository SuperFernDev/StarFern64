using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics; //to use Evaluate position
using UnityEngine;
using UnityEngine.Splines;

namespace RailShooter
{

    [System.Serializable]
    public class SplinePathData
    {
        public SliceData[] slices;
    }

    [System.Serializable]
    public class SliceData
    {
        public int splineIndex;
        public SplineRange range;

        // Can store more useful information
        public bool isEnabled = true;
        public float sliceLength;
        public float distanceFromStart;
    }

    public class PlayerFollow : MonoBehaviour
    {
        [SerializeField] SplineContainer container;
        [SerializeField] float speed = 0.04f;

        [SerializeField] SplinePathData pathData;

        SplinePath path;

        float progressRatio;
        float progress;
        float totalLength;

        // Start is called before the first frame update
        void Start()
        {
            // Get the Container's transform matrix
            Matrix4x4 localToWorldMatrix = container.transform.localToWorldMatrix;

            //Create a SplinePath from a subset of Splines (Slices)
            path = new SplinePath(CalculatePath());

            StartCoroutine(FollowCoroutine());
        }

        List<SplineSlice<Spline>> CalculatePath()
        {
            // Get the Container's transform matrix
            Matrix4x4 localToWorldMatrix = container.transform.localToWorldMatrix;

            // Get all the enabled Slices using LINQ
            List<SliceData> enabledSlices = pathData.slices.Where(slice => slice.isEnabled).ToList();

            List<SplineSlice<Spline>> slices = new List<SplineSlice<Spline>>();

            totalLength = 0f;
            foreach (var sliceData in enabledSlices)
            {
                Spline spline = container.Splines[sliceData.splineIndex];
                SplineSlice<Spline> slice = new SplineSlice<Spline>(spline, sliceData.range, localToWorldMatrix);
                slices.Add(slice);

                // Calculate the slice details
                sliceData.distanceFromStart = totalLength;
                sliceData.sliceLength = slice.GetLength();
                totalLength += sliceData.sliceLength;
            }

            return slices;
        }

        IEnumerator FollowCoroutine()
        {
            for(int n = 0; ; n++)
            {
                progressRatio = 0f;
                while (progressRatio <= 1f)
                {
                    float3 pos = path.EvaluatePosition(progressRatio);
                    float3 direction = path.EvaluateTangent(progressRatio);

                    transform.position = (Vector3)pos;
                    transform.LookAt((Vector3)(pos + direction));

                    //Increment the progress ratio
                    progressRatio += speed * Time.deltaTime;

                    //Calculate the current distance travelled
                    progress = progressRatio * totalLength;

                    yield return null;
                }
            }
        }

    }
}
