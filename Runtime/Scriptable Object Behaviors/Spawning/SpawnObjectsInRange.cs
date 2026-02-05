using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Spawn Objects In Range_ New", menuName = "Behaviors/Spawning/Spawn Objects In Range")]
public class SpawnObjectsInRange : SpawnObject
{
    [Header("Spawn Settings")][Min(0)] public float range = 1f; [Min(1)] public int count = 1; [Range(0f, 1f)] public float dropRate = 1f;
    [Space(10)]
    public float outwardForce = 0f;
    public bool flattenOutwardForce = true;
    public float upwardForce = 0f;

    [Space(10)]
    public SpawnFormation spawnFormation;

    [Header("Timing")]
    [Min(0f)]
    [Tooltip("Delay between each spawned object (seconds). 0 spawns all in the same frame.")]
    public float spawnDelayBetweenObjects = 0.01f;

    public enum SpawnFormation
    {
        RandomInsideSphere,
        RandomOnSphere,
        UniformOnSphere,
        RandomOnCircle,
        UniformOnCircle,
    }

    public static GameObject[] SpawnManyAtPosition(GameObject prefab, Vector3 position, SpawnPosition objectPosition, SpawnFormation spawnFormation, int count, float range)
    {
        GameObject[] spawnedObjects = new GameObject[count];
        List<Vector3> spawnPositions = GetSpawnPositions(spawnFormation, count);
        Vector3 offsetPosition = position + GetSpawnPositionOffset(prefab, objectPosition);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = offsetPosition + (spawnPositions[i] * range);

#if UNITY_EDITOR
            DebugExtension.DebugPoint(spawnPosition, Color.magenta, 0.15f, 10f);
#endif

            spawnedObjects[i] = Spawn(prefab, spawnPosition, Quaternion.identity, objectPosition);
        }

#if UNITY_EDITOR
        DebugExtension.DebugWireSphere(offsetPosition, Color.red, range, 10f);
#endif
        return spawnedObjects;
    }

    public static GameObject[] SpawnManyAtPositionPooled(GameObject prefab, Vector3 position, SpawnPosition objectPosition, SpawnFormation spawnFormation, int count, float range)
    {
        GameObject[] spawnedObjects = new GameObject[count];
        List<Vector3> spawnPositions = GetSpawnPositions(spawnFormation, count);
        Vector3 offsetPosition = position + GetSpawnPositionOffset(prefab, objectPosition);

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = offsetPosition + (spawnPositions[i] * range);

            spawnedObjects[i] = SpawnPooled(prefab, spawnPosition, Quaternion.identity, objectPosition);
        }

        return spawnedObjects;
    }

    private static List<Vector3> GetSpawnPositions(SpawnFormation spawnFormation, int count)
    {
        switch (spawnFormation)
        {
            case SpawnFormation.RandomInsideSphere:
                return SpawnFormationUtilities.GetRandomPointsInSphere(count);
            case SpawnFormation.RandomOnSphere:
                return SpawnFormationUtilities.GetRandomPointsOnSphere(count);
            case SpawnFormation.UniformOnSphere:
                return SpawnFormationUtilities.GetUniformPointsOnSphere(count);
            case SpawnFormation.RandomOnCircle:
                return SpawnFormationUtilities.GetRandonPointsOnCircle(count);
            case SpawnFormation.UniformOnCircle:
                return SpawnFormationUtilities.GetUniformPointsOnCircle(count);
            default:
                return SpawnFormationUtilities.GetRandomPointsInSphere(count);
        }
    }

    public static void SpawnManyAtObject(GameObject gameObject, SpawnPosition objectPosition, SpawnFormation spawnFormation, bool pool, int count, float range, GameObject prefabToSpawn, float outwardForce, bool flattenOutwardForce, float upwardForce, float perSpawnDelaySeconds)
    {
        CoWorker.Work(SpawnManyAtObjectRoutine(gameObject, objectPosition, spawnFormation, pool, count, range, prefabToSpawn, outwardForce, flattenOutwardForce, upwardForce, perSpawnDelaySeconds));
    }

    private static IEnumerator SpawnManyAtObjectRoutine(GameObject gameObject, SpawnPosition objectPosition, SpawnFormation spawnFormation, bool pool, int count, float range, GameObject prefabToSpawn, float outwardForce, bool flattenOutwardForce, float upwardForce, float perSpawnDelaySeconds)
    {
        if (gameObject == null || prefabToSpawn == null || count <= 0) yield break;
        List<Vector3> spawnPositions = GetSpawnPositions(spawnFormation, count);
        Vector3 basePosition = gameObject.transform.position;
        Vector3 offset = GetSpawnPositionOffset(prefabToSpawn, objectPosition);

        void SpawnOne(int i)
        {
            Vector3 spawnPosition = basePosition + offset + (spawnPositions[i] * range);

            GameObject spawnedObject = pool
                ? SpawnPooled(prefabToSpawn, spawnPosition, Quaternion.identity, objectPosition)
                : Spawn(prefabToSpawn, spawnPosition, Quaternion.identity, objectPosition);

            // A spawned object is null when it is networked and this is not the server
            if (spawnedObject != null)
            {
                var spawnedRigidbody = spawnedObject.GetComponentInChildren<Rigidbody>();
                if (spawnedRigidbody != null)
                {
                    Vector3 forceToAdd = Vector3.zero;

                    if (outwardForce != 0)
                    {
                        var outwardForceDirection = (spawnedObject.transform.position - basePosition).normalized;

                        if (flattenOutwardForce)
                            outwardForceDirection.FlattenY();

                        forceToAdd += outwardForceDirection * outwardForce;
                    }

                    if (upwardForce != 0)
                        forceToAdd += Vector3.up * upwardForce;

                    if (forceToAdd.magnitude > 0)
                        spawnedRigidbody.AddForce(forceToAdd, ForceMode.Impulse);
                }
            }
        }

        // Immediate spawn path (no delay).
        if (perSpawnDelaySeconds <= 0f)
        {
            for (int i = 0; i < count; i++)
                SpawnOne(i);
            yield break;
        }

        // Time-accumulator spawning (supports sub-frame multiple spawns).
        float t = 0f;
        int index = 0;
        while (index < count)
        {
            t += Time.deltaTime;

            // Spawn as many as fit in the elapsed time.
            while (t >= perSpawnDelaySeconds && index < count)
            {
                SpawnOne(index);
                index++;
                t -= perSpawnDelaySeconds;
            }

            // Yield to next frame.
            if (index < count)
                yield return null;
        }
    }


    public void SpawnManyAtObject(GameObject gameObject, SpawnPosition objectPosition)
    {
        float roll = UnityEngine.Random.value;

        if (dropRate > 0 && roll <= dropRate)
        {
            SpawnManyAtObject(gameObject, objectPosition, spawnFormation, pool, count, range, prefab, outwardForce, flattenOutwardForce, upwardForce, spawnDelayBetweenObjects);
        }
    }

    public void SpawnManyAtObject(GameObject gameObject) => SpawnManyAtObject(gameObject, SpawnPosition.Transform);
    public void SpawnManyAtObject(InteractionInfo interactionInfo) => SpawnManyAtObject(interactionInfo.interactedGameObject, SpawnPosition.Transform);
    public void SpawnManyAtObject(DamageInfo damageInfo) => SpawnManyAtObject(damageInfo.interactionInfo);
    public void SpawnManyAtObjectCenter(GameObject gameObject) => SpawnManyAtObject(gameObject, SpawnPosition.Center);
    public void SpawnManyAtObjectCenter(InteractionInfo interactionInfo) => SpawnManyAtObject(interactionInfo.interactedGameObject, SpawnPosition.Center);
    public void SpawnManyAtObjectCenter(DamageInfo damageInfo) => SpawnManyAtObjectCenter(damageInfo.interactionInfo);
    public void SpawnManyAtObjectBottom(GameObject gameObject) => SpawnManyAtObject(gameObject, SpawnPosition.Bottom);
    public void SpawnManyAtObjectBottom(InteractionInfo interactionInfo) => SpawnManyAtObject(interactionInfo.interactedGameObject, SpawnPosition.Bottom);
    public void SpawnManyAtObjectBottom(DamageInfo damageInfo) => SpawnManyAtObjectBottom(damageInfo.interactionInfo);
}
