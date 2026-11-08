using UnityEngine;
using UnityEngine.AI;

namespace Moba.Navigation
{
    /// <summary>
    /// Тонкая обёртка над NavMeshAgent для героев и миньонов.
    /// Единая точка входа для движения: клик героя, патруль бота, маршрут миньона.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMover : MonoBehaviour
    {
        private NavMeshAgent agent;

        public NavMeshAgent Agent => agent;

        public bool IsMoving =>
            agent != null && agent.isOnNavMesh && !agent.isStopped && agent.hasPath;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void Configure(float speed, float radius = 0.5f, float height = 2f)
        {
            agent.speed = speed;
            agent.acceleration = 40f;
            agent.angularSpeed = 900f;
            agent.radius = radius;
            agent.height = height;
            agent.stoppingDistance = 0.2f;
            agent.autoBraking = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        }

        public void SnapToNavMesh()
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        public void MoveTo(Vector3 destination)
        {
            if (agent == null || !agent.isOnNavMesh) return;

            agent.isStopped = false;
            agent.SetDestination(destination);
        }

        public void Stop()
        {
            if (agent == null || !agent.isOnNavMesh) return;

            agent.isStopped = true;
            agent.ResetPath();
        }

        public bool Reached(Vector3 destination, float threshold = 0.6f)
        {
            if (agent == null || !agent.isOnNavMesh || agent.pathPending) return false;
            return agent.remainingDistance <= threshold;
        }
    }
}
