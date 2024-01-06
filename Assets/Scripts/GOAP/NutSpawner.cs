using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GOAP
{
    [Serializable]
    public class NutSpawner
    {
        [SerializeField] public int nutCount = 0;
        [SerializeField] private LinkedListNode<GameObject> _head = null;
        
        public bool SpawnNut(Vector3 location)
        {
            if (!ValidateLocation(location)) return false;
            var nut = UnityEngine.Object.Instantiate(
                Resources.Load<GameObject>("Tree Nut"), 
                location, 
                Quaternion.identity);
            var nutController = nut.GetComponent<NutController>();
            nutController.spawner = this;
            
            var resNode = (_head == null) ? 
                GameModel.Nuts.AddLast(nut) : 
                GameModel.Nuts.AddAfter(_head, nut);
            _head ??= resNode;
            nutController.Node = resNode;
            
            nutCount++;
            return true;
        }

        public void RemoveNut(LinkedListNode<GameObject> node)
        {
            if (--nutCount == 0) _head = null;
            else if (node == _head) _head = _head.Next;
            GameModel.Nuts.Remove(node);
        }

        // Checks if there is space to spawn nut at location.
        // Could be optimized if we only iterated through nuts from this + nuts from player (if player then full list).
        // E.g. could place all player nuts at start of GameModel.Nuts then we just need to know how many player has spawned.
        // Another way would be to pass nutSpawners we want to check
        private bool ValidateLocation(Vector3 location)
        {
            return GameModel.Nuts.Any(nut => 
                Vector3.Distance(nut.transform.position, location) <= GameModel.NutRadius + 0.1f);
        }
    }
}