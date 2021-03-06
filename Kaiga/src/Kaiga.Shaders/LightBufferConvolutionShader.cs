﻿using System;
using Kaiga.Geom;
using Kaiga.Textures;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Core;
using Kaiga.Shaders.Fragment;
using Kaiga.Shaders.Vertex;

namespace Kaiga.Shaders
{
	public class LightBufferConvolutionShader : AbstractShader<ScreenQuadVertexShader, LightBufferConvolutionFragShader>
	{
		readonly ScreenQuadGeometry screenQuadGeom;

		public LightBufferConvolutionShader()
		{
			screenQuadGeom = new ScreenQuadGeometry();
		}

		public override void Dispose()
		{
			base.Dispose();
			screenQuadGeom.Dispose();
		}

		public void Render( ITexture2D source, int level )
		{
			BindPerPass();

			GL.ActiveShaderProgram( pipeline, fragmentShader.ShaderProgram );
			fragmentShader.SetTexture
			( 
				source.Texture, level, 
				(float)RenderParams.RenderTarget.OutputBuffer.Width / RenderParams.RenderTarget.OutputBuffer.Height
			);

			screenQuadGeom.Bind();
			screenQuadGeom.Draw();
		}
	}

	public class LightBufferConvolutionFragShader : AbstractFragmentShaderStage
	{
		public LightBufferConvolutionFragShader()
			:base( "LightBufferConvolution.frag" )
		{
		}

		public void SetTexture( int texture, int level, float aspectRatio )
		{
			SetTexture( "s_tex", texture, TextureTarget.Texture2D );
			SetUniform1( "u_mipLevel", level );
			SetUniform1( "u_aspectRatio", aspectRatio );
		}
	}
}

