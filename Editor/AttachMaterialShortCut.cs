using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.Reflection;
using Unity.Properties;

public class AttachMaterialShortCut 
{

    static void AddTexture(Texture2D texture, string propertyName, string materialName)
    {
        string shaderName = MaterialList.LoadoptionData().ShaderName;
        string materialPath = GetCurrentDirectory() + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find(shaderName));
            UnityEditor.AssetDatabase.CreateAsset(material, GetCurrentDirectory() + "/" + materialName + ".mat");
        }

        if (material.HasProperty(propertyName))
        {
            material.SetTexture(propertyName, texture);
        }
    }

  

    [MenuItem("YodaSystem/CreateYodaMaterial %t", false, 1)]
    static void OutputConsole()
    {
        string MaterialName = "";

        var objects = UnityEditor.Selection.objects;
        objects.ToList().ForEach(o =>
        {
            if(o.GetType() == typeof(Texture2D))
            {
                string[] parts = o.name.Split("_");
                
                if(MaterialName == "" || MaterialName == parts[0])   //一番最初にマテリアル名を決定する
                {
                    MaterialName = parts[0];
                    string propertyPath = "";
                    parts.ToList().Select((value, Index) =>  new {value,Index}).ToList().ForEach(t => 
                    {
                        if(t.Index != 0)
                        {
                            propertyPath += "_";
                            propertyPath += t.value;
                            
                        }
                    });

                    
                    AddTexture((Texture2D)o, propertyPath, MaterialName);
                }
            }
        });

        //マテリアル生成
        //

    }


    static string GetCurrentDirectory()
    {
        var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        var asm = Assembly.Load("UnityEditor.dll");
        var typeProjectBrowser = asm.GetType("UnityEditor.ProjectBrowser");
        var projectBrowserWindow = EditorWindow.GetWindow(typeProjectBrowser);
        return (string)typeProjectBrowser.GetMethod("GetActiveFolderPath", flag).Invoke(projectBrowserWindow, null);
    }
    



}
