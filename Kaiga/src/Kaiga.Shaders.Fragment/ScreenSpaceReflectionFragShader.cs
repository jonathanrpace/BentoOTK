using System;
using Kaiga.Core;
using System.Diagnostics;
using OpenTK.Input;
using Kaiga.Textures;

namespace Kaiga.Shaders.Fragment
{
	public class ScreenSpaceReflectionFragShader : AbstractFragmentShaderStage
	{
		readonly RandomDirectionTexture randomDirectionTexture;

		public ScreenSpaceReflectionFragShader() : base( "ScreenSpaceReflectionShader.frag" )
		{
			randomDirectionTexture = new RandomDirectionTexture();
			randomDirectionTexture.Width = 64;
			randomDirectionTexture.Height = 64;
		}

		public override void Dispose()
		{
			base.Dispose();
			randomDirectionTexture.Dispose();
		}

		public override void BindPerPass(RenderParams renderParams)
		{
			base.BindPerPass(renderParams);




		}
	}
}

