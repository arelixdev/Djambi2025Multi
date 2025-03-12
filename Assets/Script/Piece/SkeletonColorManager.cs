using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonColorManager : MonoBehaviour
{
    public List<SkinnedMeshRenderer> allSkinnedMeshRenderer = new List<SkinnedMeshRenderer>();
    public List<MeshRenderer> allMeshRenderer = new List<MeshRenderer>();
    public SkinnedMeshRenderer eyesSkinnedMeshRenderer;

    internal void SetMaterial(int team, int v)
    {
        Color teamColor = PlayerManager.Instance.colorList[v];

        foreach (var smr in allSkinnedMeshRenderer)
        {
            Material mat = PlayerManager.Instance.allSkeletonMaterials[team];
            Material newMat = new Material(mat); // Crée une instance du matériau pour éviter les modifications globales
            newMat.SetColor("_SkeletonTeamColor", teamColor); // Assigne la couleur au shader
            smr.material = newMat;
        }
        foreach (var mr in allMeshRenderer)
        {
            Material mat = PlayerManager.Instance.allSkeletonMaterials[team];
            Material newMat = new Material(mat); // Crée une instance du matériau pour éviter les modifications globales
            newMat.SetColor("_SkeletonTeamColor", teamColor); // Assigne la couleur au shader
            mr.material = newMat;
        }

        

        if(eyesSkinnedMeshRenderer.materials.Length > 1)
        {
            Material eyeMat = PlayerManager.Instance.allGlowSkeletonMaterials[team];
            Color newEmission = teamColor;
        
            eyeMat.SetColor("_EmissionColor", newEmission);
            eyesSkinnedMeshRenderer.materials[1] = eyeMat;
        } else 
        {
            Material eyeMat = PlayerManager.Instance.allGlowSkeletonMaterials[team];
            Color newEmission = teamColor * 1000;
            eyeMat.SetColor("_EmissionColor", newEmission);
            eyesSkinnedMeshRenderer.material = eyeMat;
        }
    }
}
