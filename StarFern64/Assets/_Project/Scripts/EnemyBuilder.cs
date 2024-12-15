using UnityEngine;
using UnityEngine.Splines;

namespace RailShooter
{
    public class EnemyBuilder // Builder class design pattern to build the enemies that will be spawning in the game.
                              // This pattern simplifies the construction of complex objects step by step
    {
        Transform flightPathParent;
        Enemy enemyPrefab;
        SplineContainer flightPath;
        SplineAnimate.LoopMode loopmode = SplineAnimate.LoopMode.Once;

        public EnemyBuilder withPrefab(Enemy enemyprefab)
        {
            this.enemyPrefab = enemyprefab;
            return this;
        }

        public EnemyBuilder withFlightPath(SplineContainer flightPath)
        {
            this.flightPath = flightPath;
            return this;
        }

        public EnemyBuilder withFlightPathParent(Transform flightPathParent)
        {
            this.flightPathParent = flightPathParent;
            return this;
        }

        public EnemyBuilder withLoopMode(SplineAnimate.LoopMode loopmode)
        {
            this.loopmode = loopmode;
            return this;
        }

        public Enemy build(Transform enemyParent)
        {
            Enemy enemy = Object.Instantiate(enemyPrefab, enemyParent);

            enemy.FlightPath = flightPath;

            if (flightPath != null)
            {
                SplineAnimate splineAnimate = enemy.GetComponent<SplineAnimate>();
                splineAnimate.Container = flightPath;
                
                splineAnimate.Loop = loopmode;
                splineAnimate.ElapsedTime = 0f;
            }

            if(flightPathParent != null)
            {
                flightPath.transform.SetParent(flightPathParent);
                // Reset local position and rotation
                flightPath.transform.localPosition = Vector3.zero;
                flightPath.transform.localRotation = Quaternion.identity;
            }

            return enemy;
        }

    }

}
