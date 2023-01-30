using UnityEngine;

public class ShaderToyTextureRenderer : ShaderToyRenderer
{
   public RenderTexture sourceTexture;
   public RenderTexture targetTexture;

   private void OnRenderImage(RenderTexture src, RenderTexture dest)
   {
      var from = sourceTexture ? sourceTexture : src;
      var to = targetTexture ? targetTexture : dest;
      if (RenderMaterial != null)
      {
         Graphics.Blit(from, to, RenderMaterial);
      }
      else
      {
         Graphics.Blit(from, to); 
      }
   }
}
