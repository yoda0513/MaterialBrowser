using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CreateMaterialBrowserSetting")]
public class MaterialBrowserOptionData : ScriptableObject
{
    [SerializeField]
    public List<Item> items = new List<Item>();



}

[Serializable]
public struct Item
{
    [SerializeField]
    public string path;
}