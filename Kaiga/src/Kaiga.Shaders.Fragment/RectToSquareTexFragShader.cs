using OpenTK.Graphics.OpenGL4;

namespace Kaiga.Shaders.Fragment
{
	public class RectToSquareTexFragShader : AbstractFragmentShaderStage
	{
		public void Render( int texture, int outputSize )
		{
			SetUniformTexture( 0, "s_tex", texture, TextureTarget.TextureRectangle );
			SetUniform1( "u_outputSize", outputSize );
			SetUniform1( "u_radius", 1.0f );
		}

		protected override string GetShaderSource()
		{
			return @"
#version 450 core

uniform sampler2DRect s_tex;
uniform int u_outputSize;
uniform float u_radius;

layout( location = 0 ) out vec4 out_fragColor;

void main()
{
	ivec2 texSize = textureSize( s_tex, 0 );
	float aspectRatio = texSize.x / texSize.y;

	vec2 uv = (gl_FragCoord.xy / u_outputSize) * vec2( texSize );

	vec4 sampleLeft 	= texture( s_tex, uv + vec2( -u_radius, 0.0f ) );
	vec4 sampleRight 	= texture( s_tex, uv + vec2( u_radius, 0.0f ) );
	vec4 sampleTop 		= texture( s_tex, uv + vec2( 0.0f, -u_radius ) );
	vec4 sampleBottom 	= texture( s_tex, uv + vec2( 0.0f, u_radius ) );

	out_fragColor = ( sampleLeft + sampleRight + sampleTop + sampleBottom ) / 4;
}

";
		}
	}
}

