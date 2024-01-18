using System;
using Nut;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tree
{
    public class TreeController : MonoBehaviour, IClimbable
    {
        // Climbable properties
        public bool IsOccupied { get; set; } = false;
        public float MaxHeight => GameModel.SquirrelHomeHeight;
        public Vector3 GetPointNearestTo(Vector2 loc)
        {
            var loc3 = new Vector3(loc.x, GameModel.FloorHeight, loc.y);
            return Vector3.MoveTowards(transform.position, loc3, GameModel.TreeTrunkRadius);
        }

        // public List<GameObject> nuts = new List<GameObject>(5);
        [SerializeField] private NutSpawner nutSpawner;
        [SerializeField] private int maxNutCount = 5;
        [SerializeField] private float timeSinceLastSpawn = 0f;
        [SerializeField] private float spawnDelay = 5f;
    
        // Start is called before the first frame update
        private void Start()
        {
            Random.InitState(DateTime.Now.Millisecond);
            nutSpawner = new NutSpawner();
        }

        // Update is called once per frame
        private void Update()
        {
            if (nutSpawner.nutCount < maxNutCount) timeSinceLastSpawn += Time.deltaTime;
            if (timeSinceLastSpawn >= spawnDelay && nutSpawner.SpawnNut(GETRandomNutPos())) 
                timeSinceLastSpawn = 0f; // At least 2 sec have passed and successfully spawned nut
        }

        //Returns a random point somewhere beneath the tree foliage
        private Vector3 GETRandomNutPos()
        {
            var r = Random.insideUnitCircle;
            //We want MinNutSpawnRadius <= ||r2|| <= MaxNutSpawnRadius
            var r2 = GameModel.MinNutSpawnRadius * r.normalized + (GameModel.MaxNutSpawnRadius - GameModel.MinNutSpawnRadius) * r;
            return transform.position + new Vector3(r2.x, 0f, r2.y);
        }
    }
}
