namespace Ramen
{
	public class NamedObject
	{
		public string Name { get; set; }

		public NamedObject() : this( "NamedObject" )
		{

		}

		public NamedObject( string name )
		{
			Name = name;
		}
	}
}