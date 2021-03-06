using Kaiga.Lights;
using Kaiga.Core;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders
{
	public class AmbientLightShader : AbstractShader<ScreenQuadVertexShader, AmbientLightFragShader>
	{
		public void BindPerLight( AmbientLight light )
		{
			fragmentShader.BindPerLight( light );
		}
	}

	public class AmbientLightFragShader : AbstractFragmentShaderStage
	{
		public AmbientLightFragShader() 
			: base( "LightingLib.frag", "AmbientLight.frag" )
		{
		}

		override public void BindPerPass()
		{
			base.BindPerPass();

			SetTexture( "s_materialBuffer", RenderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_albedoBuffer", RenderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_positionBuffer", RenderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_normalBuffer", RenderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
		}

		public void BindPerLight(AmbientLight light ) 
		{
			SetUniform3( "u_color", light.Color );
			SetUniform1( "u_intensity", light.Intensity );
		}
	}
}

