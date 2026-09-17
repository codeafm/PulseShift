using UnityEngine;
using UnityEditor;

public class CinematicTextureImporter:AssetPostprocessor {
 void OnPreprocessTexture(){
  if(!assetPath.Contains("/CinematicUI/")||!assetPath.EndsWith(".png"))return;
  var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.sRGBTexture=true;t.alphaIsTransparency=true;
  t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;
  t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;
  var android=t.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=2048;android.format=TextureImporterFormat.ETC2_RGBA8;android.compressionQuality=100;t.SetPlatformTextureSettings(android);
 }
}
