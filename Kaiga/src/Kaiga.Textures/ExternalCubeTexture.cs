using System;
using Kaiga.Core;
using OpenTK.TextureLoaders;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace Kaiga.Textures
{
	public class ExternalCubeTexture : AbstractValidatable, ICubeTexture
	{
		private int texture;

		public ExternalCubeTexture()
		{
			
		}

		protected override void onValidate()
		{
			TextureTarget textureTarget;

			TextureLoaderParameters.FlipImages = false;
			TextureLoaderParameters.MagnificationFilter = TextureMagFilter.Linear;
			TextureLoaderParameters.MinificationFilter = TextureMinFilter.LinearMipmapLinear;
			TextureLoaderParameters.WrapModeS = TextureWrapMode.MirroredRepeat;
			TextureLoaderParameters.WrapModeT = TextureWrapMode.MirroredRepeat;

			ImageDDS.LoadFromDisk( @"../resource/Pisa.dds", out texture, out textureTarget );

			Debug.Assert( textureTarget == TextureTarget.TextureCubeMap );
		}

		protected override void onInvalidate()
		{
			if ( GL.IsTexture( texture ) )
			{
				GL.DeleteTexture( texture );
			}
		}
		 

		#region ITexture implementation

		public int Texture
		{
			get
			{
				validate();
				return texture;
			}
		}

		#endregion
	}
}

