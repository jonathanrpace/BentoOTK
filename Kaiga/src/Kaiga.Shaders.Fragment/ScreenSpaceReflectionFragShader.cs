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

			SetRectangleTexture( "s_material", renderParams.RenderTarget.MaterialBuffer.Texture );
			SetTexture2D( "s_positionBuffer", renderParams.PositionBufferMippedTexture.Texture );
			SetTexture2D( "s_normalBuffer", renderParams.NormalBufferMippedTexture.Texture );
			SetRectangleTexture( "s_resolveBuffer", renderParams.RenderTarget.OutputBuffer.Texture );
			SetTexture2D( "s_randomDirectionTexture", randomDirectionTexture.Texture );

			SetUniformMatrix4( "u_projectionMatrix", ref renderParams.ProjectionMatrix );

			//const int numSteps = 32;
			//SetUniform1( "u_numSteps", numSteps );

			//int maxSteps = Mouse.GetState().X / 50;
			//Debug.WriteLine( maxSteps );
			//const int maxSteps =8;
			//SetUniform1( "maxSteps", maxSteps );

			//int numBinarySearchSteps = Mouse.GetState().Y / 50;
			//Debug.WriteLine( numBinarySearchSteps );
			//const int numBinarySearchSteps = 16;
			//SetUniform1( "numBinarySearchSteps", numBinarySearchSteps );

			//float roughnessJitter = (float)Mouse.GetState().Y / 1000.0f;
			//Debug.WriteLine( roughnessJitter );
			const float roughnessJitter = 0.05f;
			SetUniform1( "u_roughnessJitter", roughnessJitter );

			float zDistanceMin = (float)Mouse.GetState().Y / 2000.0f;
			Debug.WriteLine( zDistanceMin );
			//const float zDistanceMin = 1.0f;
			SetUniform1( "u_zDistanceMin", zDistanceMin );
		}
	}
}

