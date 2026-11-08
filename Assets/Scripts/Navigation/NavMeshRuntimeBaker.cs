using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Moba.Navigation
{
    /// <summary>
    /// Запекает NavMesh в рантайме после процедурной сборки карты.
    /// Отдельный сервис, чтобы карта и юниты не знали о деталях навигации.
    /// </summary>
    public static class NavMeshRuntimeBaker
    {
        public static void Bake(GameObject mapRoot)
        {
            if (mapRoot == null) return;

            NavMeshSurface surface = mapRoot.GetComponent<NavMeshSurface>();
            if (surface == null) surface = mapRoot.AddComponent<NavMeshSurface>();

            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = Physics.AllLayers;
            surface.BuildNavMesh();
        }
    }
}
