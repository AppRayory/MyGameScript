using System.Linq;
using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.EventSystems;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(ObjectRotateSystem))]
public sealed class ObjectRotateSystem : UpdateSystem
{
    private Filter _swipeFilter;
    private Filter _globalsFilter;
    private ButtonSwitcher _btnSwitcher;

    public override void OnAwake()
    {
        _btnSwitcher = FindObjectOfType<ButtonSwitcher>();
        _swipeFilter = World.Filter
            .With<SwipeDataComponent>();
        _globalsFilter = World.Filter
            .With<GlobalVarsComponent>();
    }

    public override void OnUpdate(float deltaTime)
    {
        foreach (var entity in _swipeFilter)
        {
            foreach (var _global in _globalsFilter)
            {
                if (_btnSwitcher.stateSwitcherEnum == StateSwitcherEnum.Rotate)
                {
                    ref var SwipeDataComponent = ref entity.GetComponent<SwipeDataComponent>();
                    ref var _globals = ref _global.GetComponent<GlobalVarsComponent>();
                    if (_globals.SpawnedObjects.Count > 0)
                    {
                        float rotX = Input.GetAxis("Mouse X") * (_globals.ObjectRotateSpeed * SwipeDataComponent.Distance);
                        float rotY = Input.GetAxis("Mouse Y") * (_globals.ObjectRotateSpeed * SwipeDataComponent.Distance);
                        var _rotatedObject = _globals.SpawnedObjects.ElementAt(0);
                        if (SwipeDataComponent.PressingNow)
                        {
                            switch (SwipeDataComponent.Direction)
                            {
                                case SwipeDirection.Down :
                                    _rotatedObject.transform.Rotate(Vector3.right,+rotY, Space.World);
                                    break;
                                case SwipeDirection.Up :
                                    _rotatedObject.transform.Rotate(Vector3.right,rotY, Space.World);
                                    break;
                                case SwipeDirection.Right :
                                    _rotatedObject.transform.Rotate(Vector3.down,rotX, Space.Self);
                                    break;
                                case SwipeDirection.Left :
                                    _rotatedObject.transform.Rotate(Vector3.down,+rotX, Space.Self);
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }
}