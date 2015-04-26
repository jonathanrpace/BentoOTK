using System;
using Kaiga.Core;
using OpenTK.Graphics.OpenGL4;
using Kaiga.Shaders.Vertex;
using Kaiga.Shaders.Fragment;

namespace Kaiga.Shaders
{
	public class SkyboxShader : AbstractShader
	{
		new readonly SkyboxVertexShader vertexShader;
		new readonly SkyboxFragmentShader fragmentShader;

		public SkyboxShader()
			: base( new SkyboxVertexShader(), new SkyboxFragmentShader() )
		{
			vertexShader = (SkyboxVertexShader)base.vertexShader;
			fragmentShader = (SkyboxFragmentShader)base.fragmentShader;
		}

		public void SetTexture( int texture )
		{
			fragmentShader.SetTexture( texture );
		}
	}
}

