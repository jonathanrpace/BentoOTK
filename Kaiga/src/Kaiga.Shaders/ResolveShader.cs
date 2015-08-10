using System;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;
using Kaiga.Core;
using Kaiga.Geom;

namespace Kaiga.Shaders
{
	public class ResolveShader : AbstractShader<ScreenQuadVertexShader, ResolveFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public ResolveShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render()
		{
			BindPerPass();

			fragmentShader.BindPerPass();

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
		}
	}

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
			SetRectangleTexture( "s_lightTransport", RenderParams.LightTransportRenderTarget.GetCurrentRadiosityAndAOTexture().Texture );
			SetRectangleTexture( "s_reflections", RenderParams.LightTransportRenderTarget.GetCurrentReflectionTexture().Texture );

			SetUniform1( "u_lightTransportResScalar", RenderParams.LightTransportResolutionScalar );
		}
	}
}

