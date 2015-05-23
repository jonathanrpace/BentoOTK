﻿using Kaiga.Core;
using Kaiga.Geom;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class DepthAwareBlurShader : AbstractShader<ScreenQuadVertexShader, DepthAwareBlurFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public DepthAwareBlurShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( RenderParams renderParams, int texture, int positionTexture, float radiusU, float radiusV )
		{
			BindPipeline( renderParams );
			fragmentShader.Bind( renderParams, texture, positionTexture, radiusU, radiusV );
			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
			UnbindPipeline();
		}
	}
}
