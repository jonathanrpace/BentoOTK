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
using Kaiga.Textures;
using OpenTK.Input;

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
		StandardMaterial standardMaterialA;
		StandardMaterial standardMaterialB;
		Random rand;
		AmbientLight ambientLight;

		public Program()
			: base
			(
				1920, 1080,
				new GraphicsMode( 8, 0 ), "Bento", 0,
				DisplayDevice.Default, 4, 5,
				GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug
			)
		{
			rand = new Random();
		}

		float random()
		{
			return (float)rand.NextDouble();
		}

		protected override void OnLoad( System.EventArgs e )
		{
			VSync = VSyncMode.Off;
			Context.ErrorChecking = true;

			scene = new Scene( this );

			var renderer = new DeferredRenderer();
			scene.AddProcess( renderer );
			renderer.AddRenderPass( new GPass() );
			renderer.AddRenderPass( new PointLightRenderPass() );
			renderer.AddRenderPass( new DirectionalLightRenderPass() );
			renderer.AddRenderPass( new AmbientLightRenderPass() );
			renderer.AddRenderPass( new ImageLightRenderPass() );
			//renderer.AddRenderPass( new SkyboxRenderPass() );

			const int numColumns = 8;
			const int numRows = 8;
			const float spacing = 0.1f;
			//const float radius = 0.1f;
			const float envWidth = numColumns * spacing;
			const float envDepth = numRows * spacing;
			const float envHeight = numRows * spacing;

			standardMaterialA = new StandardMaterial();
			standardMaterialA.Roughness = 0.8f;
			//standardMaterialA.Diffuse = new Vector3( 1.0f, 1.0f, 1.0f );
			standardMaterialB = new StandardMaterial();
			standardMaterialB.Roughness = 0.01f;
			standardMaterialB.Reflectivity = 1.0f;
			standardMaterialB.Diffuse = new Vector3( 1.0f, 1.0f, 1.0f );

			var sphereGeom = new SphereGeometry();
			sphereGeom.SubDivisions = 32;
			sphereGeom.Radius = 0.07f;
			var boxGeom = new BoxGeometry();
			boxGeom.Width = boxGeom.Height = boxGeom.Depth = 0.10f;;
			for ( int i = 0; i < numColumns; i++ )
			{
				var columnRatio = (float)i / ( numColumns - 1 );
				for ( int j = 0; j < numRows; j++ )
				{
					var rowRatio = (float)j / ( numRows - 1 );

					var entity = new Entity();

					if ( random() < 0.5f )
					{
						entity.AddComponent( sphereGeom );
					}
					else
					{
						entity.AddComponent( boxGeom );
					}

					var transform = new Transform();
					transform.Scale( 1.0f + random() * 2.0f );
					transform.RotateX( random() * 6.0f );
					transform.RotateY( random() * 6.0f );
					transform.RotateZ( random() * 6.0f );
					transform.Translate
					(
						(random() - 0.5f) * envWidth * 2.0f,
						(random() - 0.5f) * envHeight + envHeight * 0.5f,
						(random() - 0.5f) * envDepth * 2.0f
					);

					entity.AddComponent( transform );

					if ( random() < 0.2f )
					{
						var material = new StandardMaterial();
						material.Diffuse = new Vector3( random(), random(), random() );
						material.Emissive = 1.0f;
						material.Roughness = 1.0f;
						entity.AddComponent( material );

						var swarmMember = new SwarmMember
						( 
							new Vector3( random() * 2.0f, random() * 2.0f, random() * 2.0f ),
							new Vector3( random()* 0.5f, (float)rand.NextDouble() * 0.5f, random() * 0.5f ) 
						);
						
					
					}
					else
					{
						entity.AddComponent( standardMaterialA );
					}
					scene.AddEntity( entity );
				}
			}

			const float border = 0.5f;
			const float planeWidth = numColumns * spacing + border * 2.0f;
			const float planeHeight = numRows * spacing + border * 2.0f;
			// Floor
			{
				var entity = new Entity();

				var geom = new PlaneGeometry();
				geom.Width = planeWidth;
				geom.Height = planeHeight;
				entity.AddComponent( geom );

				var transform = new Transform();
				transform.RotateX( (float)Math.PI * 0.5f );
				entity.AddComponent( transform );

				entity.AddComponent( standardMaterialB );

				//entity.AddComponent( new EmissivePulser( 1.32f, 0.0f, 0.1f, 0.75f ) );

				scene.AddEntity( entity );
			}

			// Green wall
			{
				var entity = new Entity();

				var geom = new PlaneGeometry();
				geom.Width = planeWidth;
				geom.Height = planeHeight;
				entity.AddComponent( geom );

				var material = new StandardMaterial();
				material.Diffuse = new Vector3( 0.1f, 1.0f, 0.1f );
				material.Roughness = 0.3f;

				var transform = new Transform();
				transform.RotateY( (float)Math.PI );
				//transform.RotateX( -0.3f );
				transform.Translate( 0.0f, planeWidth * 0.5f, -planeHeight * 0.5f );
				entity.AddComponent( transform );

				entity.AddComponent( material );

				//entity.AddComponent( new EmissivePulser( 1.12f, 0.0f, 0.5f, 0.2f ) );

				scene.AddEntity( entity );
			}

			// Red wall
			{
				var entity = new Entity();

				var geom = new PlaneGeometry();
				geom.Width = planeWidth;
				geom.Height = planeHeight;
				entity.AddComponent( geom );

				var material = new StandardMaterial();
				material.Diffuse = new Vector3( 1.0f, 0.1f, 0.1f );
				material.Roughness = 0.3f;

				var transform = new Transform();
				transform.RotateY( -(float)Math.PI * 0.5f );
				transform.Translate( -planeWidth * 0.5f, planeHeight * 0.5f, 0.0f );
				entity.AddComponent( transform );

				entity.AddComponent( material );

				//entity.AddComponent( new EmissivePulser( 1.0f, 0.0f, 0.5f, 3.1f ) );

				scene.AddEntity( entity );
			}

			// Blue wall
			{
				var entity = new Entity();

				var geom = new PlaneGeometry();
				geom.Width = planeWidth;
				geom.Height = planeHeight;
				entity.AddComponent( geom );

				var transform = new Transform();
				//transform.RotateY( (float)Math.PI * -1.0f);
				transform.Translate( 0.0f, planeWidth * 0.5f, planeHeight * 0.5f );
				entity.AddComponent( transform );

				entity.AddComponent( standardMaterialB );

				//entity.AddComponent( new EmissivePulser( 1.0f, 0.0f, 0.5f, 3.1f ) );

				scene.AddEntity( entity );
			}

			for ( int i = 0; i < 20; i++ )
			{
				CreateLight();
			}

			//CreateImageLight();
			CreateAmbientLight();
			CreateDirectionalLight();

			scene.AddProcess( new OrbitCamera() );
			scene.AddProcess( new SwarmProcess() );
			scene.AddProcess( new EmissivePulseProcess() );
			scene.AddProcess( new DebugMouseDirectionalTransformProcess() );
		}

		Entity CreateLight()
		{
			var entity = new Entity();
			var transform = new Transform();
			entity.AddComponent( transform );

			var radius = 0.05f + random() * 0.05f;
			var color = new Vector3( random(), random(), random() );

			var pointLight = new PointLight();
			pointLight.Radius = radius;
			pointLight.Intensity = 5.0f;
			pointLight.AttenuationRadius = radius * 10.0f;
			pointLight.Color = color;
			//entity.AddComponent( pointLight );

			var sphereGeom = new SphereGeometry();
			sphereGeom.Radius = pointLight.Radius * 2.0f;
			//entity.AddComponent( sphereGeom );

			var material = new StandardMaterial();

			//if ( random() < 1.0f )
			//{
				material.Diffuse = color;
			//	entity.AddComponent( new EmissivePulser( 1.0f, 0.0f, 2.0f, random() * (float)Math.PI ) );
			//}

			//material.Diffuse = color;
			//material.Emissive = 1.0f;

			material.Roughness = 1.0f;//0.1f;//0.1f + random() * 0.9f;
			entity.AddComponent( material );

			var swarmMember = new SwarmMember
			( 
				new Vector3( random() * 2.0f, random() * 2.0f, random() * 2.0f ),
				new Vector3( random()* 0.5f, (float)rand.NextDouble() * 0.5f, random() * 0.5f ) 
			);
			entity.AddComponent( swarmMember );

			scene.AddEntity( entity );

			return entity;
		}

		void CreateImageLight()
		{
			var imageLight = new Entity();
			var light = new ImageLight( 0.25f );
			light.Texture = new ExternalCubeTexture();
			light.Intensity = 1.0f;
			imageLight.AddComponent( light, -1 );
			scene.AddEntity( imageLight );
		}

		void CreateAmbientLight()
		{
			var skybox = new Entity();
			var material = new SkyboxMaterial();
			material.Texture = new ExternalCubeTexture();
			skybox.AddComponent( material );
			scene.AddEntity( skybox );

			var entity = new Entity();

			ambientLight = new AmbientLight();
			ambientLight.Intensity = 0.3f;
			entity.AddComponent( ambientLight );

			scene.AddEntity( entity );
		}

		void CreateDirectionalLight()
		{
			var directionalLight = new Entity();
			var light = new DirectionalLight();
			light.Intensity = 0.8f;
			directionalLight.AddComponent( light );
			var transform = new Transform();
			directionalLight.AddComponent( transform );
			scene.AddEntity( directionalLight );
		}

		protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
		{
			scene.Dispose();
			scene = null;

			base.OnClosing(e);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			var keyboard = OpenTK.Input.Keyboard.GetState();
			if (keyboard[OpenTK.Input.Key.Escape])
				Exit();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			float roughnessA = Math.Abs( (float)OpenTK.Input.Mouse.GetState().X / 1000.0f );
			float roughnessB = Math.Abs( (float)OpenTK.Input.Mouse.GetState().Y / 1000.0f );

			//roughnessA = 1.0f;
			//standardMaterialA.Roughness = roughnessA;
			//standardMaterialB.Roughness = roughnessA;

			//ambientLight.Intensity = roughnessA;

			scene.Update( 1 / RenderFrequency );
		}
	}
}

