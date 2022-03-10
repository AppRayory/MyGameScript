using System;
using System.Collections.Generic;
using System.Linq;
using CodeMonkey.Utils;
using Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Rendering;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[CreateAssetMenu(menuName = "ECS/Systems/" + nameof(ObjectCleaningSystem))]
public sealed class ObjectCleaningSystem : UpdateSystem
{
    public enum CLEAN_TYPE
    {
        DIRT,
        BLOOD
    }

    private Filter _swipeFilter;
    private Filter _globals;
    private Filter _objectCleanerFilter;
    private Filter _needInitializeObjectFilter;
    private List<float> _getTexturesPercentList = new List<float>();
    private ButtonSwitcher _btnSwitcher;

    public Texture2D DirtMaskTexture;
    private Texture2D _brushSize;

    private Vector2Int lastPaintPixelPosition;

    private float DirtPixelsCount;
    private float DirtPixelsCountForDecrease;
    private float _texturePixelsCountForPercent;
    private float _percentPixelsCountFromTexture;
    private float _firstTexturePixelsCountForPercent;
    private float _secondTexturePixelsCountForPercent;
    private float _autoCleanedTexturePercentSubtractor;

    public override void OnAwake()
    {
        DirtPixelsCount = 0;
        _btnSwitcher = FindObjectOfType<ButtonSwitcher>();
        _swipeFilter = World.Filter
            .With<SwipeDataComponent>();
        _globals = World.Filter
            .With<GlobalVarsComponent>();
        _objectCleanerFilter = World.Filter
            .With<ObjectCleaningComponent>()
            .With<InitializedMarker>();
        _needInitializeObjectFilter = World.Filter
            .With<NeedInitializeMarker>()
            .With<ObjectCleaningComponent>();
    }

    public void InitializeTemporaryTextures()
    {
        foreach (Entity objectRef in _needInitializeObjectFilter)
        {
            _getTexturesPercentList.Clear();
            DirtPixelsCount = 0;
            _texturePixelsCountForPercent = 0;
            _percentPixelsCountFromTexture = 0;
            ref var objectCleaner = ref objectRef.GetComponent<ObjectCleaningComponent>();
            ref var globals = ref _globals.ElementAt(0).GetComponent<GlobalVarsComponent>();
            for (int i = 0; i < objectCleaner.TextureArrayBase.Count; i++)
            {
                var NexTexture = new Texture2D(
                    objectCleaner.TextureArrayBase[(CLEAN_TYPE) Enum.ToObject(typeof(CLEAN_TYPE), i)].width,
                    objectCleaner.TextureArrayBase[(CLEAN_TYPE) Enum.ToObject(typeof(CLEAN_TYPE), i)].height);
                NexTexture.SetPixels(objectCleaner.TextureArrayBase[(CLEAN_TYPE) Enum.ToObject(typeof(CLEAN_TYPE), i)]
                    .GetPixels());
                NexTexture.Apply();
                DirtPixelsCount += GetDirtyPixelsQuantity(NexTexture);
                objectCleaner.InternalTextureMutable.Add((CLEAN_TYPE) Enum.ToObject(typeof(CLEAN_TYPE), i), NexTexture);
                if (i == 0)
                {
                    objectCleaner.material.SetTexture("DirtTex", NexTexture);
                }

                if (i == 1)
                {
                    objectCleaner.material.SetTexture("DirtTex2", NexTexture);
                }
            }

            _firstTexturePixelsCountForPercent =
                GetDirtyPixelsQuantity(objectCleaner.InternalTextureMutable[CLEAN_TYPE.DIRT]);
            _secondTexturePixelsCountForPercent =
                GetDirtyPixelsQuantity(objectCleaner.InternalTextureMutable[CLEAN_TYPE.BLOOD]);

            _getTexturesPercentList.Add(_firstTexturePixelsCountForPercent);
            _getTexturesPercentList.Add(_secondTexturePixelsCountForPercent);

            DirtPixelsCountForDecrease = DirtPixelsCount;
            Debug.Log("DirtPixelsCountForDecrease = " + DirtPixelsCountForDecrease);
            BrushSizeRecalculate(ref globals);
            SetAnotherLayerToClean(ref objectCleaner);
            if (objectRef.Has<NeedInitializeMarker>())
            {
                objectRef.RemoveComponent<NeedInitializeMarker>();
            }

            if (!objectRef.Has<InitializedMarker>())
            {
                objectRef.AddComponent<InitializedMarker>();
            }
        }
    }

    [ContextMenu("Set dirt")]
    public void SetAnotherLayerToClean(ref ObjectCleaningComponent objectCleaner)
    {
        DirtMaskTexture = objectCleaner.InternalTextureMutable[CLEAN_TYPE.DIRT];
        _texturePixelsCountForPercent = GetDirtyPixelsQuantity(DirtMaskTexture);
        if (_percentPixelsCountFromTexture == 0)
        {
            _percentPixelsCountFromTexture = _texturePixelsCountForPercent;
        }
        else
        {
            var getSavedPercent = _getTexturesPercentList.Where(el
                => _percentPixelsCountFromTexture.Equals(el));
            _percentPixelsCountFromTexture = getSavedPercent.First();
        }
    }

    [ContextMenu("Set blood")]
    public void SetAnotherLayerToBlood(ref ObjectCleaningComponent objectCleaner)
    {
        DirtMaskTexture = objectCleaner.InternalTextureMutable[CLEAN_TYPE.BLOOD];
        _texturePixelsCountForPercent = GetDirtyPixelsQuantity(DirtMaskTexture);
        if (_percentPixelsCountFromTexture == 0)
        {
            _percentPixelsCountFromTexture = _texturePixelsCountForPercent;
        }
        else
        {
            var getSavedPercent = _getTexturesPercentList.Where(el
                => _percentPixelsCountFromTexture.Equals(el));
            _percentPixelsCountFromTexture = getSavedPercent.First();
        }
    }

    public override void OnUpdate(float deltaTime)
    {
        if (_needInitializeObjectFilter.Any())
        {
            InitializeTemporaryTextures();
        }

        if (!_objectCleanerFilter.Any())
        {
            return;
        }

        if (GetPixelsPercentFromOneTexture() < 10f && GetPixelsPercentFromOneTexture() > 1f)
        {
            LowPercentCleanedTextureColorChanger(DirtMaskTexture);
        }

        ref var globals = ref _globals.ElementAt(0).GetComponent<GlobalVarsComponent>();
        globals.uiText.text = Mathf.RoundToInt(GetTotalPixels() * 100f) + "%";
        if (_btnSwitcher.stateSwitcherEnum == StateSwitcherEnum.Cleaning)
        {
            ref var SwipeDataComponent = ref _swipeFilter.ElementAt(0).GetComponent<SwipeDataComponent>();
            if (SwipeDataComponent.PressingNow)
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit))
                {
                    Vector2 textureCoord = raycastHit.textureCoord;
                    float pixelX = (textureCoord.x * DirtMaskTexture.width);
                    float pixelY = (textureCoord.y * DirtMaskTexture.height);

                    int pixelXOffset = (int) pixelX - (_brushSize.width / 2);
                    int pixelYOffset = (int) pixelY - (_brushSize.height / 2);
                    for (int x = 0; x < _brushSize.width; x++)
                    {
                        for (int y = 0; y < _brushSize.height; y++)
                        {
                            Color pixelDirt = _brushSize.GetPixel(x, y);

                            if (!IsWhiteColor(pixelDirt)) continue;
                            if (IsWhiteColor(DirtMaskTexture.GetPixel(pixelXOffset + x, pixelYOffset + y))) continue;
                            DirtPixelsCountForDecrease--;
                            _texturePixelsCountForPercent--;
                            DirtMaskTexture.SetPixel(
                                pixelXOffset + x,
                                pixelYOffset + y,
                                new Color(1, 1, 1, 0)
                            );
                        }
                    }

                    Debug.LogWarning(GetPixelsPercentFromOneTexture());


                    DirtMaskTexture.Apply();
                }
            }
        }
    }

    private void LowPercentCleanedTextureColorChanger(Texture2D texture)
    {
        int counter = 0;
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                if (!IsWhiteColor(texture.GetPixel(x, y)))
                {
                    texture.SetPixel(x, y,
                        new Color(1, 1, 1, 0)
                    );
                    _texturePixelsCountForPercent--;
                    counter++;
                }
            }
        }
        DirtPixelsCountForDecrease -= counter;
        texture.Apply();
    }

    private int GetDirtyPixelsQuantity(Texture2D texture)
    {
        float pixelCount = 0;
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                if (!IsWhiteColor(texture.GetPixel(x, y)))
                {
                    pixelCount++;
                }
            }
        }

        return (int) pixelCount;
    }

    public void BrushSizeRecalculate(ref GlobalVarsComponent _global)
    {
        if (_btnSwitcher.brushSwitcherEnum == BrushSwitcherEnum.GetSmallBrush)
        {
            _brushSize = _global.SmallBrush;
        }
        else if (_btnSwitcher.brushSwitcherEnum == BrushSwitcherEnum.GetMiddleBrush)
        {
            _brushSize = _global.MiddleBrush;
        }

        if (_btnSwitcher.brushSwitcherEnum == BrushSwitcherEnum.GetLargeBrush)
        {
            _brushSize = _global.LargeBrush;
        }
    }

    private float GetTotalPixels()
    {
        return DirtPixelsCountForDecrease / DirtPixelsCount;
    }

    private float GetPixelsPercentFromOneTexture()
    {
        return _texturePixelsCountForPercent / _percentPixelsCountFromTexture * 100;
    }

    private bool IsWhiteColor(Color color)
    {
        if (color.r != 1) return false;
        if (color.g != 1) return false;
        if (color.b != 1) return false;
        return true;
    }
}