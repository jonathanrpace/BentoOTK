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
using Kaiga.Lights;

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
			renderer.AddRenderPass( new PointLightRenderPass() );

			var rand = new Random();
			for ( int i = 0; i < 30; i++ )
			{
				var entity = new Entity();

				var geom = new SphereGeometry();
				geom.Radius = 0.01f + (float)rand.NextDouble() * 0.2f;
				entity.AddComponent( geom );

				var material = new StandardMaterial();
				material.glossiness = (float)rand.NextDouble();
				material.reflectivity = 1.0f;//(float)rand.NextDouble();
				entity.AddComponent( material );

				var transform = new Transform();
				const float positionScale = 1.0f;
				transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( 
					(float)(rand.NextDouble()-0.5f)*positionScale, 
					(float)(rand.NextDouble()-0.5f)*positionScale, 
					(float)(rand.NextDouble()-0.5f)*positionScale );
				entity.AddComponent( transform );

				scene.AddEntity( entity );
			}

			for ( int i = 0; i < 1; i++ )
			{
				var entity = new Entity();
				var transform = new Transform();
				const float positionScale = 0.0f;
				transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( 
					(float)(rand.NextDouble()-0.5f)*positionScale, 
					(float)(rand.NextDouble()-0.5f)*positionScale, 
					(float)(rand.NextDouble()-0.5f)*positionScale );
				entity.AddComponent( transform );

				var attenuationExp = 1.0f;//(float)rand.NextDouble() * 100.0f + 100.0f;
				var lightComponent = new PointLight( new Vector3( (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble() ), 1.0f, 1.0f, 1.0f, attenuationExp );

				entity.AddComponent( lightComponent );

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

