using System;

namespace Kaiga.Util
{
	public static class TextureUtil
	{
		static readonly int MAX_SIZE = 2048;
		static readonly int MIN_SIZE = 4;

		public static int GetNumMipMaps( int size )
		{
			size = GetBestPowerOf2( size );

			if ( size < MIN_SIZE )
				return 0;

			int numMipMaps = 1;
			while ( size > MIN_SIZE )
			{
				size >>= 1;
				numMipMaps++;
			}

			return numMipMaps-1;
		}

		public static bool IsDimensionValid( int d )
		{
				return d >= MIN_SIZE && d <= MAX_SIZE && IsPowerOfTwo(d);
		}

		public static bool IsPowerOfTwo( int value )
		{
			return value > 0 && ( ( value & -value ) == value );
		}

		public static int GetBestPowerOf2( int value )
		{
			int p = 1;

			while (p < value)
				p <<= 1;

			if (p > MAX_SIZE) p = MAX_SIZE;

			return p;
		}
	}
}

