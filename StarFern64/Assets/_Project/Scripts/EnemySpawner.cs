using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Shapes;
using UnityEngine.Splines;

namespace RailShooter
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] Annulus[] annuli;
        [SerializeField] Disc[] discs;

        [SerializeField] Enemy enemyPrefab;
        [SerializeField] float spawnInterval = 5f;

        [SerializeField] Transform enemyParent;
        [SerializeField] Transform flightPathParent;

        float spawnTimer;

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < annuli.Length; i++)
            {
                discs[i].transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + annuli[i].distance);
                discs[i].Radius = annuli[i].outerRadius;
                discs[i].Thickness = annuli[i].outerRadius - annuli[i].innerRadius;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(spawnTimer > spawnInterval)
            {
                spawnTimer = 0f;
                SpawnEnemy();
            }
            spawnTimer += Time.deltaTime;
        
        }

        private void SpawnEnemy()
        {
            SplineContainer flightPath = FlightPathFactory.GenerateFlightPath(annuli);
            EnemyFactory.GenerateEnemy(enemyPrefab, flightPath, enemyParent, flightPathParent);

        }

    }
}
