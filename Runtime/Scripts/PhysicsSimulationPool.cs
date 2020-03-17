using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using OhmsLibraries.Pooling;
namespace OhmsLibraries.PhysicsSimulation {
    public class PhysicsSimulationPool : Pool<PhysicsSimulatedObject> {
        [InfoBox( "Only the first element in the list PoolMonobehaviours will be used." )]
        protected override void InstantiateObjects() {
            pool = new List<PhysicsSimulatedObject>( poolSize );
            poolQueue = new Queue<PhysicsSimulatedObject>();
            for ( int i = 0; i < poolSize; i++ ) {
                var pgo = Instantiate( PoolMonoBehaviours[0] );
                pool.Add( pgo );
                poolQueue.Enqueue( pgo );
                pgo.Available = true;
                pgo.OnPoolReturnRequest = ReturnToQueue;
            }
        }

        public void InitPool() {
            InstantiateObjects();
        }
    }
}