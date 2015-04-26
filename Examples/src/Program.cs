﻿using System;
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
		Random rand;

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

		protected override void OnLoad( System.EventArgs e )
		{
			VSync = VSyncMode.Off;
			Context.ErrorChecking = true;

			scene = new Scene( this );

			var renderer = new DeferredRenderer();
			scene.AddProcess( renderer );
			renderer.AddRenderPass( new GPass() );
			renderer.AddRenderPass( new PointLightRenderPass() );
			renderer.AddRenderPass( new AmbientLightRenderPass() );
			renderer.AddRenderPass( new ImageLightRenderPass() );
			renderer.AddRenderPass( new AORenderPass() );
			renderer.AddRenderPass( new SkyboxRenderPass() );
			renderer.AddRenderPass( new LightResolvePass() );

			const int numColumns = 10;
			const int numRows = 10;
			const float spacing = 0.2f;
			const float radius = 0.1f;

			for ( int i = 0; i < numColumns; i++ )
			{
				var columnRatio = (float)i / ( numColumns - 1 );
				for ( int j = 0; j < numRows; j++ )
				{
					var rowRatio = (float)j / ( numRows - 1 );

					var entity = new Entity();
					
					var geom = new SphereGeometry();
					geom.Radius = radius;
					geom.SubDivisions = 32;
					entity.AddComponent( geom );

					var material = new StandardMaterial();
					material.Roughness = 0.1f + columnRatio * 0.9f;
					material.Reflectivity = rowRatio;
					entity.AddComponent( material );

					var transform = new Transform();
					transform.Matrix = Matrix4.Identity * Matrix4.CreateTranslation( 
						(i-numColumns*0.5f) * spacing, 
						0.0f, 
						(j-numRows*0.5f) * spacing );
					entity.AddComponent( transform );

					scene.AddEntity( entity );
				}
			}

			{
				float border = 0.5f;
				var floor = new Entity();
				var floorGeom = new PlaneGeometry();
				floorGeom.Width = numColumns * spacing + border * 2.0f;
				floorGeom.Height = numRows * spacing + border * 2.0f;
				floor.AddComponent( floorGeom );
				var transform = new Transform();
				transform.RotateX( (float)Math.PI * 0.5f );
				transform.Translate( -radius, -radius, -radius );
				floor.AddComponent( transform );
				var floorMaterial = new StandardMaterial();
				floorMaterial.Roughness = 0.75f;
				floor.AddComponent( floorMaterial );
				scene.AddEntity( floor );
			}

			{
				var skybox = new Entity();
				var material = new SkyboxMaterial();
				material.Texture = new ExternalCubeTexture();
				skybox.AddComponent( material );
				scene.AddEntity( skybox );

				var imageLight = new Entity();
				var light = new ImageLight();
				light.Texture = material.Texture;
				imageLight.AddComponent( light );
				scene.AddEntity( imageLight );
			}

			for ( int i = 0; i < 10; i++ )
			{
				CreateLight();
			}

			//CreateAmbientLight();

			scene.AddProcess( new OrbitCamera() );
			scene.AddProcess( new SwarmProcess() );
		}

		Entity CreateLight()
		{
			var entity = new Entity();
			var transform = new Transform();
			entity.AddComponent( transform );

			var radius = 0.01f + (float)rand.NextDouble() * 0.1f;
			var color = new Vector3( (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble() );

			var pointLight = new PointLight();
			pointLight.Radius = radius;
			pointLight.Intensity = 10.0f;
			pointLight.Color = color;
			entity.AddComponent( pointLight );

			var sphereGeom = new SphereGeometry();
			sphereGeom.Radius = pointLight.Radius;
			entity.AddComponent( sphereGeom );

			var material = new StandardMaterial();
			material.Diffuse = color;
			material.Emissive = 0.2f;
			entity.AddComponent( material );

			var swarmMember = new SwarmMember( 
				                  	new Vector3( (float)rand.NextDouble() * 2.0f, (float)rand.NextDouble() * 2.0f, (float)rand.NextDouble() * 2.0f ),
									new Vector3( (float)rand.NextDouble() * 0.5f, (float)rand.NextDouble() * 0.5f, (float)rand.NextDouble() * 0.5f ) 
			                  );
			entity.AddComponent( swarmMember );

			scene.AddEntity( entity );

			return entity;
		}

		Entity CreateAmbientLight()
		{
			var entity = new Entity();

			var ambientLight = new AmbientLight( 1.0f, 1.0f, 1.0f, 0.1f );
			entity.AddComponent( ambientLight );

			scene.AddEntity( entity );

			return entity;
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
			scene.Update( 1 / RenderFrequency );
		}
	}
}

