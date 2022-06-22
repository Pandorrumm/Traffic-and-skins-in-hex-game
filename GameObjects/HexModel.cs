using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static GameEntity.HexOfWorld;
using GameData;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HexModel : SerializedMonoBehaviour
{
    public MeshRenderer meshRenderer;

    public Dictionary<Side, int> materialIndex;

    [OnValueChanged("ChangeTopLeft")]
    public Material topLeft;
    [OnValueChanged("ChangeTopRight")]
    public Material topRight;
    [OnValueChanged("ChangeLeft")]
    public Material left;
    [OnValueChanged("ChangeRight")]
    public Material right;
    [OnValueChanged("ChangeBottomLeft")]
    public Material bottomLeft;
    [OnValueChanged("ChangeBottomRight")]
    public Material bottomRight;

    private void ChangeTopLeft()
    {
        ChangeHexSide(Side.TopLeft, ref topLeft);
    }

    private void ChangeTopRight()
    {
        ChangeHexSide(Side.TopRight, ref topRight);
    }

    private void ChangeLeft()
    {
        ChangeHexSide(Side.Left, ref left);
    }

    private void ChangeRight()
    {
        ChangeHexSide(Side.Right, ref right);
    }

    private void ChangeBottomLeft()
    {
        ChangeHexSide(Side.BottomLeft, ref bottomLeft);
    }

    private void ChangeBottomRight()
    {
        ChangeHexSide(Side.BottomRight, ref bottomRight);
    }

    private void ChangeHexSide(Side side, ref Material material)
    {
        Material[] newMaterials = new Material[6];
        
        for(int i = 0; i < newMaterials.Length; i++) {
            if (materialIndex.ContainsKey(side) && materialIndex[side] == i) {
                newMaterials[i] = material;
            } else if (materialIndex[side] != i) {
                newMaterials[i] = meshRenderer.sharedMaterials[i];
            }
        }

        meshRenderer.sharedMaterials = newMaterials;
        meshRenderer.UpdateGIMaterials();
    }

    public void UpdateSides(CellOfWorldData data)
    {
        foreach(KeyValuePair<Side, SideDataAndConditions> sideData in data.sidesData) {
            ChangeHexSide(sideData.Key, ref sideData.Value.sideData.material);
        }
    }

    public Side GetCorrectSide(int index)
    {
        foreach(KeyValuePair<Side, int> keyValue in materialIndex) {
            if (keyValue.Value == index) return keyValue.Key;
        }

        Debug.LogError("Error in HexModel method GetCorrectSide(int index)!!!");
        return Side.Left;
    }

#if UNITY_EDITOR
    [Button("Update render")]
    private void ApplyChanges()
    {
        StartCoroutine(cr_Wait());
    }

    private IEnumerator cr_Wait()
    {
        yield return new WaitForEndOfFrame();
        Selection.SetActiveObjectWithContext(meshRenderer.gameObject, meshRenderer.gameObject);
        yield return new WaitForEndOfFrame();
        Selection.SetActiveObjectWithContext(this.gameObject, this.gameObject);
    }
    #endif
}
