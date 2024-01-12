using System.Collections.Generic;
using UnityEngine;

namespace Nut
{
    public class NutController : MonoBehaviour
    {
        public LinkedListNode<NutController> Node;
        public NutSpawner spawner;
        
        public void PickUp()
        {
            spawner.RemoveNut(Node);
            Destroy(gameObject);
        }
    }
}