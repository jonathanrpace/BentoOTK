namespace Kaiga.Shaders.Fragment
{
	public class NullFragShader : AbstractFragmentShaderStage
	{
		override protected string GetShaderSource()
		{
			return @"
			#version 440 core

			void main(void)
			{ 
				
			}
			";
		}

	}
}

