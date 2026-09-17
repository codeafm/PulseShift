using UnityEngine;
using System;

[CreateAssetMenu(menuName="PULSESHIFT/Reference model")]
public class ReferenceModelAsset:ScriptableObject {
 public string referenceId;
 public Material material;
 public ReferencePart[] parts;
}
[Serializable] public class ReferencePart {public string name;public Mesh mesh;public Vector3 pivot;}
