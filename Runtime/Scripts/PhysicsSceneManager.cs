using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
namespace OhmsLibraries.PhysicsSimulation {
    [RequireComponent( typeof( PhysicsSimulationPool ) )]
    public class PhysicsSceneManager : MonoBehaviour {

        public event System.Action<SimulationData> OnSimluationStep;
        public event System.Action<List<SimulationData>> OnSimulationEnd;
        public event System.Action<int> OnSimulationStart;

        public bool simulate = false;
        public bool simulateOnce;
        [Required, ValidateInput( "Editor_HasRigidbody", "Object to Simulate must contain a Rigidbody" )]
        public GameObject objectToSimulate;
        [MinValue( 0.3f )]
        public float timeToSimulate;
        [Tooltip( "Creates this objects in the physics scene and interact in the simulation." )]
        public GameObject[] enviromentObjects;

        public Vector3 velocity;
        public bool useGravity;
        public ForceMode simulatedForce;

        [SerializeField]
        private PhysicsTimeConfig timeConfig = PhysicsTimeConfig.USE_FIXED_DELTA_TIME;
        [SerializeField, Range( 0.001f, 0.03f )]
        public float customTimeStep = 0.3f;

        [HideInEditorMode, DisableInPlayMode]
        public List<SimulationData> data = new List<SimulationData>();

        private PhysicsSimulationPool _pool;
        private PhysicsSimulationScene _scenes;
        private Rigidbody _simulatedRigidBody;

        private int Steps {
            get => Mathf.FloorToInt( timeToSimulate / TimeStep );
        }

        private float TimeStep {
            get {
                if ( timeConfig == PhysicsTimeConfig.CUSTOM ) {
                    return customTimeStep;
                }
                return Time.fixedDeltaTime;
            }
        }

#if UNITY_EDITOR
        private bool Editor_HasRigidbody( GameObject g ) {
            if ( !g ) return true;
            return g.GetComponent<Rigidbody>();
        }

        private bool Editor_PhysicsTypeNotNone( LocalPhysicsMode mode ) {
            return mode != LocalPhysicsMode.None;
        }

        private bool Editor_CustomTime {
            get => timeConfig == PhysicsTimeConfig.CUSTOM;
        }
#endif

        private void Awake() {
            Physics.autoSimulation = false;
            var pScene = SceneManager.CreateScene( "Physics", new CreateSceneParameters( LocalPhysicsMode.Physics3D ) );
            var gScene = SceneManager.GetActiveScene();
            _scenes = new PhysicsSimulationScene( pScene, gScene );
            _pool = gameObject.GetComponent<PhysicsSimulationPool>();
            _simulatedRigidBody = objectToSimulate.GetComponent<Rigidbody>();
        }

        private void Start() {
            InstatiateOnSimulationScene();
        }

        private void FixedUpdate() {
            if ( simulate ) {
                simulate = !simulateOnce;
                Simulation();
            }
            _scenes.SimulateGameScene( Time.fixedDeltaTime );
        }

        private void Simulation() {
            SimulationData current = new SimulationData {
                position = objectToSimulate.transform.position,
                rotation = objectToSimulate.transform.rotation
            };
            _scenes.BeginSimulation();

            if ( _pool.RequestPoolMonoBehaviour( out PhysicsSimulatedObject obj ) ) {
                obj.Spawn( current.position, current.rotation );

                obj.body.AddForce( velocity, simulatedForce );
                obj.body.useGravity = useGravity;

                var steps = Steps;
                var time = TimeStep;
                AddData( obj.transform );
                OnSimulationStart?.Invoke( steps );
                for ( int i = 0; i < steps; i++ ) {
                    _scenes.SimulatePhysicsScene( time );
                    AddData( obj.transform );
                }
                OnSimulationEnd?.Invoke( data );
                data.Clear();

                obj.Despawn();
            }

            _scenes.EndSimulation();
        }

        private void InstatiateOnSimulationScene() {
            _scenes.BeginSimulation();

            _pool.InitPool();
            for ( int i = 0; i < enviromentObjects.Length; i++ ) {
                var enviroment = Instantiate( enviromentObjects[i] );
                var enviromentRenderer = enviroment.GetComponent<Renderer>();
                if ( enviromentRenderer ) Destroy( enviromentRenderer );
            }

            _scenes.EndSimulation();
        }

        private void AddData( Transform simulationTransform ) {
            SimulationData simData = new SimulationData {
                position = simulationTransform.position,
                rotation = simulationTransform.rotation
            };
            //DebugGraph.Log( "Simulated Position", simData.position );
            //Debug.Log( $"Position {simData.position}" );
            OnSimluationStep?.Invoke( simData );
            data.Add( simData );
        }
    }

    public enum PhysicsTimeConfig {
        USE_FIXED_DELTA_TIME, CUSTOM
    }

    public struct SimulationData {
        public Vector3 position;
        public Quaternion rotation;
    }

    public class PhysicsSimulationScene {

        private Scene _physicsScene, _gameScene;
        private PhysicsScene _simulateScene, _gameSimuateScene;

        public PhysicsSimulationScene( Scene physicsScene, Scene gameScene ) {
            _physicsScene = physicsScene;
            _gameScene = gameScene;
            _simulateScene = physicsScene.GetPhysicsScene();
            _gameSimuateScene = _gameScene.GetPhysicsScene();
        }

        public void BeginSimulation() {
            SceneManager.SetActiveScene( _physicsScene );
        }

        public void EndSimulation() {
            SceneManager.SetActiveScene( _gameScene );
        }

        public void SimulatePhysicsScene( float time ) {
            _simulateScene.Simulate( time );
        }

        public void SimulateGameScene( float time ) {
            _gameSimuateScene.Simulate( time );
        }
    }
}