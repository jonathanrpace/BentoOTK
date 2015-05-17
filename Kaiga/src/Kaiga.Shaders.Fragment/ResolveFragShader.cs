using Kaiga.Core;

namespace Kaiga.Shaders.Fragment
{
	public class ResolveFragShader : AbstractFragmentShaderStage
	{
		public ResolveFragShader() : base( "ResolveShader.frag" )
		{
		}

		public override void BindPerPass(RenderParams renderParams)
		{
			base.BindPerPass(renderParams);

			SetRectangleTexture( "s_directLight", renderParams.DirectLightTextureRect.Texture );
			SetRectangleTexture( "s_indirectLight", renderParams.IndirectLightTextureRect.Texture );
			SetRectangleTexture( "s_lightTransport", renderParams.LightTransportRenderTarget.RadiosityAndAOTextureRect.Texture );

			SetUniform1( "u_lightTransportResScalar", renderParams.LightTransportResolutionScalar );
		}
	}
}

