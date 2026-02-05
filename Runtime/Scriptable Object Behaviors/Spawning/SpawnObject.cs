using FishNet;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Spawn Object_ New", menuName = "Behaviors/Spawning/Spawn Object")]
public class SpawnObject : ScriptableObject 
{
	[Header("Prefab")]
	public GameObject prefab;
	public SpawnPosition spawnPosition;

	[Header("Spawn Settings")]
	public bool pool = false;
	[Tooltip("If true, the spawned object will inherit the rotation when spawned at an object.")]
	public bool inheritGameObjectRotation;

	public Vector3 positionOffset;

	public enum SpawnPosition
	{
		Transform,
		Center,
		Bottom,
	}


	public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, SpawnPosition spawnPosition = SpawnPosition.Transform, Scene spawnScene = default)
	{

		GameObject spawnedObject = null;

		if (prefab.TryGetComponent(out NetworkObject networkObject))
		{
			if (InstanceFinder.NetworkManager != null && InstanceFinder.NetworkManager.IsServerStarted)
			{
				spawnedObject = Instantiate(prefab, position, rotation);

				InstanceFinder.ServerManager.Spawn(spawnedObject, scene: spawnScene);
			}
		}
		else
		{
			spawnedObject = Instantiate(prefab, position, rotation);

			if (spawnScene.IsValid())
                SceneManager.MoveGameObjectToScene(spawnedObject, spawnScene);
		}


		return spawnedObject;
	}

	public static GameObject SpawnPooled(GameObject prefab, Vector3 position, Quaternion rotation, SpawnPosition spawnPosition = SpawnPosition.Transform, Scene spawnScene = default)
	{
        GameObject spawnedObject = null;

		if (prefab.TryGetComponent(out NetworkObject networkObject))
		{
			if (InstanceFinder.NetworkManager != null && InstanceFinder.NetworkManager.IsServerStarted)
			{
				spawnedObject = SimplePool.Spawn(prefab, position, rotation);

				InstanceFinder.ServerManager.Spawn(spawnedObject, scene: spawnScene);
			}
		}
		else
		{
			spawnedObject = SimplePool.Spawn(prefab, position, rotation);

			if (spawnScene.IsValid())
                SceneManager.MoveGameObjectToScene(spawnedObject, spawnScene);
		}

        return spawnedObject;
	}

	public virtual GameObject Spawn(Vector3 position, Quaternion rotation)
	{
		var spawnPos = position + positionOffset;

        if (prefab != null)
			return pool ? SpawnPooled(prefab, spawnPos, rotation, spawnPosition) : Spawn(prefab, spawnPos, rotation, spawnPosition);
			
		return null;
	}

	public virtual void Spawn() => Spawn(Vector3.zero, Quaternion.identity);

	public virtual void SpawnAtPosition(Vector3 position) => Spawn(position, Quaternion.identity);
	public virtual void SpawnAtPosition(Vector3 position, Quaternion rotation) => Spawn(position, rotation);
	public virtual void SpawnAtTransform(Transform transform) => Spawn(transform.position, transform.rotation);
    public virtual void SpawnAtObject(MonoBehaviour monoBehaviour) => Spawn(monoBehaviour.transform.position + GetSpawnPositionOffset(monoBehaviour.gameObject, spawnPosition), inheritGameObjectRotation ? monoBehaviour.transform.rotation : Quaternion.identity);
	public virtual void SpawnAtObject(GameObject gameObject) => Spawn(gameObject.transform.position + GetSpawnPositionOffset(gameObject, spawnPosition), inheritGameObjectRotation ? gameObject.transform.rotation : Quaternion.identity);
	public void SpawnAtObject(NetworkObject networkObject) => SpawnAtObject(networkObject.gameObject);
    public void SpawnAtInteractingObject(InteractionInfo interactionInfo) => SpawnAtObject(interactionInfo.interactingGameObject);
	public void SpawnAtInteractingObject(DamageInfo damageInfo) => SpawnAtInteractingObject(damageInfo.interactionInfo);
	public void SpawnAtInteractedObject(InteractionInfo interactionInfo) => SpawnAtObject(interactionInfo.interactedGameObject);
	public void SpawnAtInteractedObject(DamageInfo damageInfo) => SpawnAtInteractedObject(damageInfo.interactionInfo);
	public virtual void SpawnAtContact(ContactInfo contactInfo) => Spawn(contactInfo.point, Quaternion.LookRotation(contactInfo.normal));
	public virtual void SpawnAtContactingObject(ContactInfo contactInfo) => Spawn(contactInfo.contactingGameObject.transform.position, contactInfo.contactingGameObject.transform.rotation);
	public void SpawnAtInteractedCollider(Collider other, Collider collider) => Spawn(collider.transform.position, Quaternion.identity);
	public void SpawnAtInteractingCollider(Collider other, Collider collider) => Spawn(other.transform.position, Quaternion.identity);


	protected static Vector3 GetSpawnPositionOffset(GameObject gameObject, SpawnPosition spawnPosition)
	{
		Vector3 spawnPositionOffset = Vector3.zero;

		if (spawnPosition == SpawnPosition.Center)
		{
			Bounds objectBounds = gameObject.GetRendererBounds();
			Vector3 objectCenter = gameObject.transform.InverseTransformPoint(objectBounds.center);

			spawnPositionOffset = objectCenter;
		}
		else if (spawnPosition == SpawnPosition.Bottom)
		{
			Bounds objectBounds = gameObject.GetRendererBounds();
			Vector3 objectCenter = gameObject.transform.InverseTransformPoint(objectBounds.center);
			Vector3 bottomPosition = objectCenter - Vector3.up * objectBounds.extents.y;

			spawnPositionOffset = bottomPosition;
		}

		return spawnPositionOffset;
	}
}