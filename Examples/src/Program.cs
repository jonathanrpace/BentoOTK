using System;
using OpenTK;
using OpenTK.Graphics;
using Ramen;
using Kaiga.Core;
using Kaiga.RenderPasses;
using Kaiga.Geom;
using Kaiga.Processes;
using Kaiga.Components;
using Kaiga.Materials;

namespace Examples
{
	public class Program : GameWindow
	{
		[STAThread]
		public static void Main()
		{
			using (Program program = new Program())
			{
				program.Run(60);
			}
		}

		private Scene scene;

		public Program()
			: base
			( 
				800, 600,
				new GraphicsMode(), "Bento", 0,
				DisplayDevice.Default, 4, 5,
				GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug
			)
		{

		}

		protected override void OnLoad( System.EventArgs e )
		{
			VSync = VSyncMode.On;
			Context.ErrorChecking = true;

			scene = new Scene( this );

			var renderer = new DeferredRenderer();
			scene.AddProcess( renderer );
			renderer.AddRenderPass( new TestRenderPass() );

			var rand = new Random();
			for ( int i = 0; i < 50; i++ )
			{
				var entity = new Entity();

				var geom = new SphereGeometry();
				geom.Radius = (float)rand.NextDouble() * 0.2f;
				entity.AddComponent( geom );

				var material = new StandardMaterial();
				entity.AddComponent( material );

				var transform = new Transform();
				transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( (float)rand.NextDouble()-0.5f, (float)rand.NextDouble()-0.5f, (float)rand.NextDouble()-0.5f );
				entity.AddComponent( transform );

				scene.AddEntity( entity );
			}

			scene.AddProcess( new GraphicsContextDependencyManager() );
			scene.AddProcess( new OrbitCamera() );
		}

		protected override void OnUnload( System.EventArgs e )
		{
			scene.Clear();
			scene = null;

			base.OnUnload(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			//Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
			//Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
			//GL.UniformMatrix4(modelviewMatrixLocation, false, ref modelviewMatrix);

			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape])
				Exit();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			scene.Update( 1 / RenderFrequency );
		}
	}
}

