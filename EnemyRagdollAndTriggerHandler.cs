using System.Collections;
using System.Collections.Generic;
using Morpeh;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRagdollAndTriggerHandler : RagdollHandler
{
    private Entity _enemyEntity;
    [HideInInspector]
    public bool isPlayerFollowing;
    [HideInInspector]
    public bool isTakeDamage;
    public AddedPeopleCountManager AddedSlapCountUI;
    public GlobalData              GlobalData;
    private void Start()
    {
        _ragdollObject.gameObject.SetActive(false);
        _enemyEntity      = gameObject.GetComponent<EnemyProvider>().Entity;
        GlobalData        = FindObjectOfType<GlobalData>();
        isPlayerFollowing = false;
        isTakeDamage      = false;

        if (AddedSlapCountUI == null){
            AddedSlapCountUI = FindObjectOfType<AddedPeopleCountManager>();
        }
        
    }

    protected override void GetRagdollBonesTransform(Transform destinationTransform)
    {
        var randomVector3 = new Vector3(Random.Range(-6.0f, 6.0f)
            , Random.Range(-6.0f, 6.0f), Random.Range(-6.0f, 6.0f));
        if (gameObject.TryGetComponent(out NavMeshAgent agent)){
            agent.enabled = false;
        }
        for (int i = 0; i < destinationTransform.childCount; i++)
        {
            var destination = destinationTransform.GetChild(i);
            var rb = destination.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.maxAngularVelocity = Mathf.Infinity;
                rb.velocity = transform.up * Random.Range(1, 2);
                rb.angularVelocity = randomVector3 * Random.Range(0, 10);
            }
            GetRagdollBonesTransform(destination); 
        }
    }

    public void ResetBonesPosAfterDie(Transform ragdollBones, Transform animatedBones)
    {
        for (int i = 0; i < ragdollBones.childCount; i++)
        {
            var ragdollBone = ragdollBones.GetChild(i);
            var animatedBone = animatedBones.GetChild(i);
            ragdollBone.position = animatedBone.position;
            ragdollBone.rotation = animatedBone.rotation;
            ResetBonesPosAfterDie(ragdollBone, animatedBone);
        }
    }
    
    protected override void PushRagdollBonesForward(Transform destinationTransform) {
        var randomVector3 = new Vector3(Random.Range(-6.0f, 6.0f)
            , Random.Range(-6.0f, 6.0f), Random.Range(-6.0f, 6.0f));
        _ragdollObject.transform.SetParent(FindObjectOfType<SpawnerProvider>().GetData().EnemyContainer);
        for (int i = 0; i < destinationTransform.childCount; i++)
        {
            var destination = destinationTransform.GetChild(i);
            var rb = destination.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.maxAngularVelocity = Mathf.Infinity;
                rb.velocity = transform.forward * Random.Range(10, 15)  + transform.up * Random.Range(5, 10);
                rb.angularVelocity = randomVector3 * Random.Range(0, 10);
            }
            PushRagdollBonesForward(destination); 
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<SawTriggerMarker>())
        {
            if (_enemyEntity.Has<EnemyToMovingMarker>()){
                GetRagdollBody(); // Метод наследуемого класса
                gameObject.GetComponent<EnemyProvider>().GetData().NavMeshAgent.enabled = false;
                _enemyEntity.RemoveComponent<EnemyToMovingMarker>();
            }
        }
        if (other.gameObject.GetComponent<SlapHandler>())
        {
            if (!_enemyEntity.Has<EnemyToMovingMarker>() && !_enemyEntity.Has<PlayerTakenDamageMarker>()) {
                if (isTakeDamage)
                {
                    AddedSlapCountUI.ShowNewCount("+1");
                    GlobalData.AddSlap();
                    GetRagdollBody(); // Метод наследуемого класса
                    if (!_enemyEntity.Has<EnemyToSpawnMarker>())
                    {
                        _enemyEntity.AddComponent<EnemyToSpawnMarker>();
                    }
                }
            }

            if (_enemyEntity.Has<PlayerTakenDamageMarker>())
            {
                GetRagdollBodyAfterDamage(); // Метод наследуемого класса
            }
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.GetComponent<GroundMarker>())
        {
            if (_enemyEntity.Has<EnemyToJumpMarker>())
            {
                if (!_enemyEntity.Has<EnemyToMovingMarker>())
                {
                    gameObject.GetComponent<EnemyProvider>().GetData().EnemyRigidBody.isKinematic = true;
                    gameObject.GetComponent<EnemyProvider>().GetData().EnemyCollider.isTrigger = true;
                    gameObject.GetComponent<EnemyProvider>().GetData().NavMeshAgent.enabled = true;
                    _enemyEntity.AddComponent<EnemyToMovingMarker>();
                    _enemyEntity.RemoveComponent<EnemyToJumpMarker>();
                }
            }
        }
    }
}
