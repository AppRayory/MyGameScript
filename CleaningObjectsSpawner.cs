using System.Collections.Generic;
using System.Linq;
using Morpeh;
using UnityEngine;
using Random = UnityEngine.Random;

public class CleaningObjectsSpawner : MonoBehaviour
{
    
    [SerializeField] private Transform objectsParent;
    [SerializeField] private List<GameObject> cleaningObjects;
    
    private GlobalVarsComponent _globals;
    
    private void Start()
    {
        _globals = FindObjectOfType<GlobalVarsProvider>().GetData();
    }

    
    public void SpawnObject()
    {
        if (cleaningObjects.Count == 0) {
            Debug.LogWarning("There is no objects for spawn");
            return;
        }
        if (_globals.SpawnedObjects.Count > 0) {
            for (int i = 0; i < _globals.SpawnedObjects.Count; i++) {
                var hidenEntity = _globals.SpawnedObjects[i].GetComponent<ObjectCleaningProvider>().Entity;
                _globals.SpawnedObjects[i].SetActive(false);
                _globals.SpawnedObjects.Remove(_globals.SpawnedObjects[i]);
                if (hidenEntity.Has<InitializedMarker>()) {
                    hidenEntity.RemoveComponent<InitializedMarker>();
                }
            }
        }
        var getRandomObjectIndex = Random.Range(0, cleaningObjects.Count);
        GameObject objectForRotation = Instantiate(cleaningObjects[getRandomObjectIndex], objectsParent);
        var objectCleanerScript = objectForRotation.GetComponent<ObjectCleaningProvider>().GetData();
        var entity = objectForRotation.GetComponent<ObjectCleaningProvider>().Entity;
        objectCleanerScript.material.SetTexture("_MainTex",objectCleanerScript.MainTex);
        objectForRotation.SetActive(true);
        _globals.SpawnedObjects.Add(objectForRotation);
        cleaningObjects.Remove(cleaningObjects.ElementAt(getRandomObjectIndex));
        if (!entity.Has<NeedInitializeMarker>()) {
            entity.AddComponent<NeedInitializeMarker>();
        }
    }
}
