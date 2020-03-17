using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using OhmsLibraries.Pooling;
namespace OhmsLibraries.PhysicsSimulation {
    [RequireComponent( typeof( Rigidbody ) )]
    public class PhysicsSimulatedObject : PoolMonoBehaviour {
        [HideInNonPrefabs]
        public Rigidbody body;
        private Vector3 _originPosition;

#if UNITY_EDITOR
        private void Reset() {
            body = GetComponent<Rigidbody>();
        }
#endif
        private void Awake() {

        }

        public override void Spawn( Vector3 position, Quaternion rotation ) {
            base.Spawn( position, rotation );
            _originPosition = position;

        }

        public override void Spawn( Vector3 position ) {
            base.Spawn( position );
            _originPosition = position;
        }

        public override void Despawn() {
            base.Despawn();
            transform.position = _originPosition;
        }
    }
}