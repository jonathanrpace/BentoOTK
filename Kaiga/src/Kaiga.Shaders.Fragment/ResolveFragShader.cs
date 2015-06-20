using Kaiga.Core;

namespace Kaiga.Shaders.Fragment
{
	public class ResolveFragShader : AbstractFragmentShaderStage
	{
		public ResolveFragShader() : base( "ResolveShader.frag" )
		{
		}

		public override void BindPerPass()
		{
			base.BindPerPass();

			SetRectangleTexture( "s_directLight", RenderParams.DirectLightTextureRect.Texture );
			SetRectangleTexture( "s_indirectLight", RenderParams.IndirectLightTextureRect.Texture );
			SetRectangleTexture( "s_lightTransport", RenderParams.LightTransportRenderTarget.RadiosityAndAOTextureRect.Texture );
			SetRectangleTexture( "s_reflections", RenderParams.LightTransportRenderTarget.ReflectionsTextureRect.Texture );

			SetUniform1( "u_lightTransportResScalar", RenderParams.LightTransportResolutionScalar );
		}
	}
}

