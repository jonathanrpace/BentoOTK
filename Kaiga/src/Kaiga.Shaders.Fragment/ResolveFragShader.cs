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

			SetRectangleTexture( "s_directLight", renderParams.RenderTarget.DirectLightBuffer.Texture );
			SetRectangleTexture( "s_indirectLight", renderParams.RenderTarget.IndirectLightBuffer.Texture );
			SetRectangleTexture( "s_material", renderParams.RenderTarget.MaterialBuffer.Texture );
			SetRectangleTexture( "s_lightTransport", renderParams.AORenderTarget.AOBuffer.Texture );

			SetUniform1( "u_lightTransportResScalar", renderParams.LightTransportResolutionScalar );
		}
	}
}

