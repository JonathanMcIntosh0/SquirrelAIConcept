using System.Collections.Generic;
using Tree;
using UnityEngine;

namespace GOAP
{
    public class NutController : MonoBehaviour
    {
        public LinkedListNode<GameObject> Node;
        public NutSpawner spawner;
        
        public void PickUp()
        {
            spawner.RemoveNut(Node);
            Destroy(gameObject);
        }
    }
}