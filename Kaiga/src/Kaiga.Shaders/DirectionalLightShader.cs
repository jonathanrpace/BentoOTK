using Kaiga.Lights;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class DirectionalLightShader : AbstractShader<ScreenQuadVertexShader, DirectionalLightFragShader>
	{
		public void BindPerLight( RenderParams renderParams, DirectionalLight light )
		{
			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.BindPerLight( renderParams, light );
		}
	}

	public class DirectionalLightFragShader : AbstractFragmentShaderStage
	{
		public DirectionalLightFragShader() : base( "LightingLib.frag", "DirectionalLightShader.frag" )
		{
		}

		override public void BindPerPass( RenderParams renderParams )
		{
			base.BindPerPass( renderParams );

			SetTexture( "s_positionBuffer", renderParams.RenderTarget.PositionBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_materialBuffer", renderParams.RenderTarget.MaterialBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_albedoBuffer", renderParams.RenderTarget.AlbedoBuffer.Texture, TextureTarget.TextureRectangle );
			SetTexture( "s_normalBuffer", renderParams.RenderTarget.NormalBuffer.Texture, TextureTarget.TextureRectangle );
		}

		public void BindPerLight( RenderParams renderParams, DirectionalLight light )
		{
			SetUniform1( "u_radius", light.Radius );
			SetUniform3( "u_color", light.Color );
			
			SetUniform1( "u_intensity", light.Intensity );
			SetUniform3( "u_lightPosition", renderParams.ModelViewMatrix.ExtractTranslation() );
		}

	}

}

