using System;
using System.Linq;
using Morpeh;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSwitcher : MonoBehaviour
{
    [SerializeField] private Button RotateObjectButton;
    [SerializeField] private Button DrawingObjectButton;
    [SerializeField] private Button SpawnObjectButton;
    [Header("BrushesButtons")]
    [Space(10)]
    [SerializeField] private Button GetSmallBrushBtn;
    [SerializeField] private Button GetMiddleBrushBtn;
    [SerializeField] private Button GetLargeBrushBtn;

    private GlobalVarsComponent _globals;
    private CleaningObjectsSpawner _objectsSpawner;
    
    public StateSwitcherEnum stateSwitcherEnum;
    public BrushSwitcherEnum brushSwitcherEnum;

    private void Awake() {
        _objectsSpawner = FindObjectOfType<CleaningObjectsSpawner>();
        RotateObjectButton.onClick.AddListener(() => SwitchStateToRotateObject());
        DrawingObjectButton.onClick.AddListener(() => SwitchStateToCleaning());
        SpawnObjectButton.onClick.AddListener(() => SpawningObject());
        GetSmallBrushBtn.onClick.AddListener(() => GetSmallBrush());
        GetMiddleBrushBtn.onClick.AddListener(() => GetMiddleBrush());
        GetLargeBrushBtn.onClick.AddListener(() => GetLargetBrush());
    }

    private void Start()
    {
        _globals = FindObjectOfType<GlobalVarsProvider>().GetData();
    }

    public void SpawningObject() {
        _objectsSpawner.SpawnObject();
    }
    
    public void SwitchStateToRotateObject() {
        stateSwitcherEnum = StateSwitcherEnum.Rotate;
    }

    public void SwitchStateToCleaning() {
        stateSwitcherEnum = StateSwitcherEnum.Cleaning;
    }
    
    public void GetSmallBrush() {
        brushSwitcherEnum = BrushSwitcherEnum.GetSmallBrush;
        var a = 
            FindObjectOfType<Installer>().updateSystems
                .First(el => el.System.GetType() == typeof(ObjectCleaningSystem)).System as ObjectCleaningSystem ; 
        a.BrushSizeRecalculate(ref _globals);
    }

    public void GetMiddleBrush() {
        brushSwitcherEnum = BrushSwitcherEnum.GetMiddleBrush;
        var a = 
            FindObjectOfType<Installer>().updateSystems
                .First(el => el.System.GetType() == typeof(ObjectCleaningSystem)).System as ObjectCleaningSystem ; 
        a.BrushSizeRecalculate(ref _globals);
    }
    public void GetLargetBrush() {
        brushSwitcherEnum = BrushSwitcherEnum.GetLargeBrush;
        var a = 
            FindObjectOfType<Installer>().updateSystems
                .First(el => el.System.GetType() == typeof(ObjectCleaningSystem)).System as ObjectCleaningSystem ; 
        a.BrushSizeRecalculate(ref _globals);
    }
    
    [ContextMenu("Set dirt")]
    public void SetAnotherLayerToClean()
    {
        var spawnedObject = FindObjectsOfType<ObjectCleaningProvider>();
        foreach (var objectCleaning in spawnedObject)
        {
            if (objectCleaning.gameObject.activeInHierarchy)
            {
                var a =
                    FindObjectOfType<Installer>().updateSystems
                        .First(el => el.System.GetType() ==
                                     typeof(ObjectCleaningSystem)).System as ObjectCleaningSystem;
                a.SetAnotherLayerToClean(ref objectCleaning.GetData());
                SwitchStateToCleaning();
            }
        }
    }

    [ContextMenu("Set blood")]
    public void SetAnotherLayerToblood()
    {
        var spawnedObject = FindObjectsOfType<ObjectCleaningProvider>();
        foreach (var objectCleaning in spawnedObject)
        {
            if (objectCleaning.gameObject.activeInHierarchy)
            {
                var a =
                    FindObjectOfType<Installer>().updateSystems
                        .First(el => el.System.GetType() == typeof(ObjectCleaningSystem))
                        .System as ObjectCleaningSystem;
                a.SetAnotherLayerToBlood(ref objectCleaning.GetData());
                SwitchStateToCleaning();
            }
        }
    }
}


