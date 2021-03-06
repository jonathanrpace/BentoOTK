using Kaiga.Lights;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using OpenTK;
using Kaiga.Components;

namespace Kaiga.Shaders
{
	public class DirectionalLightShader : AbstractShader<ScreenQuadVertexShader, DirectionalLightFragShader>
	{
		public void BindPerLight( DirectionalLight light, Transform transform )
		{
			fragmentShader.BindPerLight( light, transform );
		}
	}

	public class DirectionalLightFragShader : AbstractFragmentShaderStage
	{
		public DirectionalLightFragShader() 
			: base( "LightingLib.frag", "DirectionalLight.frag" )
		{
		}

		override public void BindPerPass()
		{
			base.BindPerPass();

			SetTexture( "s_positionBuffer", RenderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_materialBuffer", RenderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_albedoBuffer", RenderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_normalBuffer", RenderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
		}

		public void BindPerLight( DirectionalLight light, Transform transform )
		{
			SetUniform3( "u_color", light.Color );
			
			SetUniform1( "u_intensity", light.Intensity );
			SetUniform3( "u_lightPosition", RenderParams.ModelViewMatrix.ExtractTranslation() );
			SetUniform3( "u_lightDirection", transform.Forward() );
		}

	}

}

