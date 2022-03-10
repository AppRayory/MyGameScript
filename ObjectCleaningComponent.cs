using System.Collections.Generic;
using Morpeh;
using TMPro;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

[Il2CppSetOption(Option.NullChecks, false)]
[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
[Il2CppSetOption(Option.DivideByZeroChecks, false)]
[System.Serializable]
public struct ObjectCleaningComponent : IComponent {
    
     public                  Texture2D                                                          dirtMaskTextureBase;
     public                  Texture2D                                                          MainTex;
     public                  Material                                                           material;
     [SerializeField] public SerializableDictionary<ObjectCleaningSystem.CLEAN_TYPE, Texture2D> TextureArrayBase;
     [SerializeField] public SerializableDictionary<ObjectCleaningSystem.CLEAN_TYPE, Texture2D> InternalTextureMutable;
}