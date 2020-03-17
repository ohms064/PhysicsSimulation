using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace OhmsLibraries.PhysicsSimulation {
    public class LineRendererPath : MonoBehaviour {
        public LineRenderer lineRenderer;
        public PhysicsSceneManager manager;

        private void OnEnable() {
            manager.OnSimulationEnd += Manager_OnSimulationEnd;
            manager.OnSimulationStart += Manager_OnSimulationStart;
        }

        private void OnDisable() {
            manager.OnSimulationEnd -= Manager_OnSimulationEnd;
            manager.OnSimulationStart -= Manager_OnSimulationStart;
        }

        void Manager_OnSimulationEnd( List<SimulationData> obj ) {
            var array = (from o in obj select o.position).ToArray();
            lineRenderer.SetPositions( array );
        }

        void Manager_OnSimulationStart( int steps ) {
            lineRenderer.positionCount = 0;
            lineRenderer.positionCount = steps;
        }

    }
}