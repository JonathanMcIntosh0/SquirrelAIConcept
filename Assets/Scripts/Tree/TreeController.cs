using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tree
{
    public class TreeController : MonoBehaviour, IClimbable
    {
        // Climbable properties
        public bool IsOccupied { get; set; } = false;
        public float MaxHeight => GameModel.SquirrelHomeHeight;

        public List<GameObject> nuts = new List<GameObject>(5);
        private float _timeSinceLastSpawn = 0f;
    
        // Start is called before the first frame update
        void Start()
        {
            Random.InitState(DateTime.Now.Millisecond);
        }

        // Update is called once per frame
        void Update()
        {
            _timeSinceLastSpawn += Time.deltaTime;
            if (_timeSinceLastSpawn >= 2f) //at least 2 sec have passed
            {
                SpawnNut();
            }
        }

        public bool SpawnNut()
        {
            if (nuts.Count >= 5) return false;
            var nut = Instantiate(Resources.Load("Tree Nut", typeof(GameObject))) as GameObject;
            nut.transform.position = getRandomNutPos();
        
            nuts.Add(nut);

            //Note: we only reach here if nuts.count < 5 thus we will insta spawn a nut if we drop below max allowance (of course
            //only if more than 2 seconds have passed after the last nut spawn) 
            _timeSinceLastSpawn = 0f; 
            return true;
        }

        //Returns a random point somewhere beneath the tree foliage such that we are not overlapping with any other nuts
        private Vector3 getRandomNutPos()
        {
            Vector3 newP;
            do
            {
                var r = Random.insideUnitCircle;
                //We want MinNutSpawnRadius <= ||r2|| <= MaxNutSpawnRadius
                var r2 = GameModel.MinNutSpawnRadius * r.normalized + (GameModel.MaxNutSpawnRadius - GameModel.MinNutSpawnRadius) * r;
                newP = transform.position + new Vector3(r2.x, 0f, r2.y);
            } while (CheckNutOverlap(newP));

            return newP;
        }

        //Returns true if one of the existing nuts overlaps with a new nut at point p
        private bool CheckNutOverlap(Vector3 p)
        {
            return nuts.Any(nut => 
                Vector3.Distance(nut.transform.position, p) <= GameModel.NutRadius + 0.1f);
        }

    }
}
