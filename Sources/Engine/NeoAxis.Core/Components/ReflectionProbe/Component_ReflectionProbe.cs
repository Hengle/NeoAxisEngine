// Copyright (C) NeoAxis Group Ltd. 8 Copthall, Roseau Valley, 00152 Commonwealth of Dominica.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using NeoAxis.Editor;

namespace NeoAxis
{
	/// <summary>
	/// The camera that captures surroundings in the scene and passes information to the reflective surfaces.
	/// </summary>
	[EditorSettingsCell( typeof( Component_ReflectionProbe_SettingsCell ) )]
	public class Component_ReflectionProbe : Component_ObjectInSpace
	{
		//Component_Image captureTexture;
		//bool captureTextureNeedUpdate = true;

		bool processedCubemapNeedUpdate = true;
		Component_Image processedEnvironmentCubemap;
		Component_Image processedIrradianceCubemap;

		/////////////////////////////////////////

		public enum ModeEnum
		{
			Resource,
			Capture,
		}

		/// <summary>
		/// The mode of the reflection probe.
		/// </summary>
		[DefaultValue( ModeEnum.Resource )]//Capture )]
		public Reference<ModeEnum> Mode
		{
			get { if( _mode.BeginGet() ) Mode = _mode.Get( this ); return _mode.value; }
			set
			{
				if( _mode.BeginSet( ref value ) )
				{
					try
					{
						ModeChanged?.Invoke( this );
						//if( Mode.Value != ModeEnum.Capture )
						//	CaptureTextureDispose();
						processedCubemapNeedUpdate = true;
					}
					finally { _mode.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Mode"/> property value changes.</summary>
		public event Action<Component_ReflectionProbe> ModeChanged;
		ReferenceField<ModeEnum> _mode = ModeEnum.Resource;// Capture;

		//!!!!����� ColorMultiplier?
		///// <summary>
		///// The brightness of the reflection probe.
		///// </summary>
		//[DefaultValue( 1.0 )]
		//[Range( 0, 2 )]
		//public Reference<double> Brightness
		//{
		//	get { if( _brightness.BeginGet() ) Brightness = _brightness.Get( this ); return _brightness.value; }
		//	set { if( _brightness.BeginSet( ref value ) ) { try { BrightnessChanged?.Invoke( this ); } finally { _brightness.EndSet(); } } }
		//}
		//public event Action<Component_ReflectionProbe> BrightnessChanged;
		//ReferenceField<double> _brightness = 1;

		/// <summary>
		/// The cubemap texture of reflection data used by the probe.
		/// </summary>
		[Category( "Resource" )]
		[DefaultValueReference( @"Samples\Starter Content\Environments\Forest.image" )]
		public Reference<Component_Image> Cubemap
		{
			get { if( _cubemap.BeginGet() ) Cubemap = _cubemap.Get( this ); return _cubemap.value; }
			set
			{
				if( _cubemap.BeginSet( ref value ) )
				{
					try
					{
						CubemapChanged?.Invoke( this );
						processedCubemapNeedUpdate = true;
					}
					finally { _cubemap.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Cubemap"/> property value changes.</summary>
		public event Action<Component_ReflectionProbe> CubemapChanged;
		ReferenceField<Component_Image> _cubemap = new Reference<Component_Image>( null, @"Samples\Starter Content\Environments\Forest.image" );

		///// <summary>
		///// The cubemap texture of irradiance data used by the probe.
		///// </summary>
		//[Category( "Resource" )]
		//public Reference<Component_Image> CubemapIrradiance
		//{
		//	get { if( _cubemapIrradiance.BeginGet() ) CubemapIrradiance = _cubemapIrradiance.Get( this ); return _cubemapIrradiance.value; }
		//	set { if( _cubemapIrradiance.BeginSet( ref value ) ) { try { CubemapIrradianceChanged?.Invoke( this ); } finally { _cubemapIrradiance.EndSet(); } } }
		//}
		//public event Action<Component_ReflectionProbe> CubemapIrradianceChanged;
		//ReferenceField<Component_Image> _cubemapIrradiance;

		//!!!!remove
		///// <summary>
		///// Whether to capture and pass the information in real-time.
		///// </summary>
		//[DefaultValue( false )]
		//[Category( "Capture" )]
		//public Reference<bool> Realtime
		//{
		//	get { if( _realtime.BeginGet() ) Realtime = _realtime.Get( this ); return _realtime.value; }
		//	set { if( _realtime.BeginSet( ref value ) ) { try { RealtimeChanged?.Invoke( this ); } finally { _realtime.EndSet(); } } }
		//}
		//public event Action<Component_ReflectionProbe> RealtimeChanged;
		//ReferenceField<bool> _realtime = false;

		public enum ResolutionEnum
		{
			_1,
			_2,
			_4,
			_8,
			_16,
			_32,
			_64,
			_128,
			_256,
			_512,
			_1024,
			_2048,
			//_4096,
			//_8192,
			//_16384,//!!!!��� ��������� ������ �� ������
		}

		/// <summary>
		/// The resolution of the capture.
		/// </summary>
		[DefaultValue( ResolutionEnum._256 )]
		[Category( "Capture" )]
		public Reference<ResolutionEnum> Resolution
		{
			get { if( _resolution.BeginGet() ) Resolution = _resolution.Get( this ); return _resolution.value; }
			set
			{
				if( _resolution.BeginSet( ref value ) )
				{
					try
					{
						ResolutionChanged?.Invoke( this );
						//CaptureTextureDispose();
					}
					finally { _resolution.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="Resolution"/> property value changes.</summary>
		public event Action<Component_ReflectionProbe> ResolutionChanged;
		ReferenceField<ResolutionEnum> _resolution = ResolutionEnum._256;

		/// <summary>
		/// Whether the high dynamic range is enabled.
		/// </summary>
		[DefaultValue( true )]
		[Category( "Capture" )]
		public Reference<bool> HDR
		{
			get { if( _hdr.BeginGet() ) HDR = _hdr.Get( this ); return _hdr.value; }
			set
			{
				if( _hdr.BeginSet( ref value ) )
				{
					try
					{
						HDRChanged?.Invoke( this );
						//CaptureTextureDispose();
					}
					finally { _hdr.EndSet(); }
				}
			}
		}
		/// <summary>Occurs when the <see cref="HDR"/> property value changes.</summary>
		public event Action<Component_ReflectionProbe> HDRChanged;
		ReferenceField<bool> _hdr = true;

		//!!!!remove
		///// <summary>
		///// Whether to generate a mip levels.
		///// </summary>
		//[DefaultValue( false )]
		//[Category( "Capture" )]
		//public Reference<bool> Mipmaps
		//{
		//	get { if( _mipmaps.BeginGet() ) Mipmaps = _mipmaps.Get( this ); return _mipmaps.value; }
		//	set { if( _mipmaps.BeginSet( ref value ) ) { try { MipmapsChanged?.Invoke( this ); } finally { _mipmaps.EndSet(); } } }
		//}
		//public event Action<Component_ReflectionProbe> MipmapsChanged;
		//ReferenceField<bool> _mipmaps = false;

		/// <summary>
		/// The minimum distance of the reflection probe.
		/// </summary>
		[DefaultValue( 0.1 )]
		[Category( "Capture" )]
		[Range( 0.01, 1 )]
		public Reference<double> NearClipPlane
		{
			get { if( _nearClipPlane.BeginGet() ) NearClipPlane = _nearClipPlane.Get( this ); return _nearClipPlane.value; }
			set { if( _nearClipPlane.BeginSet( ref value ) ) { try { NearClipPlaneChanged?.Invoke( this ); } finally { _nearClipPlane.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="NearClipPlane"/> property value changes.</summary>
		public event Action<Component_ReflectionProbe> NearClipPlaneChanged;
		ReferenceField<double> _nearClipPlane = 0.1;

		/// <summary>
		/// The maximum distance of the reflection probe.
		/// </summary>
		[DefaultValue( 100.0 )]
		[Category( "Capture" )]
		[Range( 10, 10000, RangeAttribute.ConvenientDistributionEnum.Exponential, 3 )]
		public Reference<double> FarClipPlane
		{
			get { if( _farClipPlane.BeginGet() ) FarClipPlane = _farClipPlane.Get( this ); return _farClipPlane.value; }
			set { if( _farClipPlane.BeginSet( ref value ) ) { try { FarClipPlaneChanged?.Invoke( this ); } finally { _farClipPlane.EndSet(); } } }
		}
		/// <summary>Occurs when the <see cref="FarClipPlane"/> property value changes.</summary>
		public event Action<Component_ReflectionProbe> FarClipPlaneChanged;
		ReferenceField<double> _farClipPlane = 100;

		//!!!!
		//capture settings:
		//Resolution - � ������ ��� ����� ��������� �����. ����� �� ������ �������
		//Shadow Distance
		//Background Color
		//Use Occlusion Culling
		//�� CubemapZone ��� ���?

		protected override void OnMetadataGetMembersFilter( Metadata.GetMembersContext context, Metadata.Member member, ref bool skip )
		{
			base.OnMetadataGetMembersFilter( context, member, ref skip );

			if( member is Metadata.Property )
			{
				switch( member.Name )
				{
				case nameof( Cubemap ):
					//case nameof( CubemapIrradiance ):
					if( Mode.Value != ModeEnum.Resource )
						skip = true;
					break;

				case nameof( Resolution ):
				case nameof( HDR ):
				//case nameof( Mipmaps ):
				//case nameof( Realtime ):
				case nameof( NearClipPlane ):
				case nameof( FarClipPlane ):
					if( Mode.Value != ModeEnum.Capture )
						skip = true;
					break;
				}
			}
		}

		protected override void OnEnabled()
		{
			base.OnEnabled();

			processedCubemapNeedUpdate = true;
		}

		protected override void OnSpaceBoundsUpdate( ref SpaceBounds newBounds )
		{
			base.OnSpaceBoundsUpdate( ref newBounds );

			var tr = Transform.Value;
			var attenuationFar = tr.Scale.MaxComponent();
			newBounds = new SpaceBounds( new Sphere( tr.Position, attenuationFar ) );
		}

		//protected override void OnTransformChanged()
		//{
		//	base.OnTransformChanged();

		//	captureTextureNeedUpdate = true;
		//}

		[Browsable( false )]
		public Sphere Sphere
		{
			get { return SpaceBounds.CalculatedBoundingSphere; }
		}

		protected override bool OnEnabledSelectionByCursor()
		{
			if( !ParentScene.GetDisplayDevelopmentDataInThisApplication() || !ParentScene.DisplayLabels )
				return false;
			//if( !ParentScene.GetShowDevelopmentDataInThisApplication() || !ParentScene.ShowReflectionProbes )
			//	return false;
			return base.OnEnabledSelectionByCursor();
		}

		void DebugDrawBorder( Viewport viewport )
		{
			var sphere = Sphere;
			var pos = sphere.Origin;
			var r = sphere.Radius;

			const double angleStep = Math.PI / 32;
			for( double angle = 0; angle < Math.PI * 2 - angleStep * .5; angle += angleStep )
			{
				double p1sin = Math.Sin( angle );
				double p1cos = Math.Cos( angle );
				double p2sin = Math.Sin( angle + angleStep );
				double p2cos = Math.Cos( angle + angleStep );

				//����� ������ ����� ��������. ��� ��� 45 ��������

				viewport.Simple3DRenderer.AddLine( pos + ( new Vector3( p1cos, p1sin, 0 ) * r ), pos + ( new Vector3( p2cos, p2sin, 0 ) * r ) );
				viewport.Simple3DRenderer.AddLine( pos + ( new Vector3( 0, p1cos, p1sin ) * r ), pos + ( new Vector3( 0, p2cos, p2sin ) * r ) );
				viewport.Simple3DRenderer.AddLine( pos + ( new Vector3( p1cos, 0, p1sin ) * r ), pos + ( new Vector3( p2cos, 0, p2sin ) * r ) );
			}
		}

		public void DebugDraw( Viewport viewport )
		{
			var sphere = Sphere;
			var pos = sphere.Origin;
			var r = sphere.Radius;

			DebugDrawBorder( viewport );

			viewport.Simple3DRenderer.AddLine( pos - new Vector3( r, 0, 0 ), pos + new Vector3( r, 0, 0 ) );
			viewport.Simple3DRenderer.AddLine( pos - new Vector3( 0, r, 0 ), pos + new Vector3( 0, r, 0 ) );
			viewport.Simple3DRenderer.AddLine( pos - new Vector3( 0, 0, r ), pos + new Vector3( 0, 0, r ) );
		}

		protected override void OnUpdate( float delta )
		{
			base.OnUpdate( delta );

			UpdateProcessedCubemaps();
		}

		public override void OnGetRenderSceneData( ViewportRenderingContext context, GetRenderSceneDataMode mode )
		{
			if( mode == GetRenderSceneDataMode.InsideFrustum )
			{
				//UpdateProcessedCubemaps();

				var sphere = Sphere;
				if( sphere.Radius > 0 )
				{
					var item = new Component_RenderingPipeline.RenderSceneData.ReflectionProbeItem();
					item.Creator = this;
					item.BoundingBox = SpaceBounds.CalculatedBoundingBox;
					//item.BoundingSphere = SpaceBounds.CalculatedBoundingSphere;
					item.Sphere = sphere;

					//if( Mode.Value == ModeEnum.Resource )
					//{
					if( processedEnvironmentCubemap != null )
						item.CubemapEnvironment = processedEnvironmentCubemap;
					else
						item.CubemapEnvironment = Cubemap;

					item.CubemapIrradiance = processedIrradianceCubemap;

					//item.CubemapEnvironment = Cubemap;
					//item.CubemapIrradiance = CubemapIrradiance;
					//}
					//else
					//{
					//	//!!!!IBL
					//	item.CubemapEnvironment = CaptureTexture;
					//}

					context.FrameData.RenderSceneData.ReflectionProbes.Add( item );
				}

				{
					var context2 = context.objectInSpaceRenderingContext;

					bool show = ( ParentScene.GetDisplayDevelopmentDataInThisApplication() && ParentScene.DisplayReflectionProbes ) || context2.selectedObjects.Contains( this ) || context2.canSelectObjects.Contains( this ) || context2.objectToCreate == this;
					if( show )
					{
						if( context2.displayReflectionProbesCounter < context2.displayReflectionProbesMax )
						{
							context2.displayReflectionProbesCounter++;

							ColorValue color;
							if( context2.selectedObjects.Contains( this ) )
								color = ProjectSettings.Get.SelectedColor;
							else if( context2.canSelectObjects.Contains( this ) )
								color = ProjectSettings.Get.CanSelectColor;
							else
								color = ProjectSettings.Get.SceneShowReflectionProbeColor;

							var viewport = context.Owner;
							if( viewport.Simple3DRenderer != null )
							{
								viewport.Simple3DRenderer.SetColor( color, color * ProjectSettings.Get.HiddenByOtherObjectsColorMultiplier );
								DebugDraw( viewport );
								//viewport.Simple3DRenderer.AddSphere( Transform.Value.ToMat4(), 0.5, 32 );
							}
						}
					}
					//if( !show )
					//	context.disableShowingLabelForThisObject = true;
				}
			}
		}

		//public bool Contains( Vec3 point )
		//{
		//	var scale = Transform.Value.Scale;
		//	var radius = Math.Max( scale.X, Math.Max( scale.Y, scale.Z ) ) * 0.5;
		//	return new Sphere( Transform.Value.Position, radius ).Contains( point );
		//}

		//public virtual void GetOutputTextures( out Component_Texture texture, out Component_Texture textureIBL )
		//{
		//	texture = null;
		//	textureIBL = null;

		//	//!!!!���� ���

		//	if( SourceType.Value == SourceTypeEnum.SpecifiedCubemap )
		//	{
		//		texture = Cubemap;
		//		textureIBL = CubemapIBL;
		//	}
		//}

		//protected override void OnDisabled()
		//{
		//	CaptureTextureDispose();
		//	base.OnDisabled();
		//}

		//void CaptureTextureUpdate()
		//{
		//	if( Mode.Value == ModeEnum.Capture )
		//	{
		//		var resolution = Resolution.Value;
		//		var hdr = HDR.Value;
		//		var mipmaps = Mipmaps.Value;

		//		if( captureTexture != null && captureTexture.Disposed )
		//			captureTexture = null;
		//		if( captureTexture == null )
		//		{
		//			var size = int.Parse( resolution.ToString().Replace( "_", "" ) );
		//			PixelFormat format = hdr ? PixelFormat.Float16RGBA : PixelFormat.A8R8G8B8;

		//			var texture = ComponentUtility.CreateComponent<Component_Image>( null, true, false );
		//			captureTexture = texture;
		//			texture.CreateType = Component_Image.TypeEnum.Cube;
		//			texture.CreateSize = new Vector2I( size, size );
		//			texture.CreateMipmaps = mipmaps;
		//			//texture.CreateMipmaps = true;
		//			//texture.CreateArrayLayers = arrayLayers;
		//			texture.CreateFormat = format;
		//			texture.CreateUsage = Component_Image.Usages.RenderTarget;
		//			texture.CreateFSAA = 0;// fsaaLevel;
		//			texture.Enabled = true;

		//			//!!!!

		//			//int faces = type == Component_Image.TypeEnum.Cube ? 6 : arrayLayers;

		//			//int numMips;
		//			//if( mipmaps )
		//			//{
		//			//	int max = size.MaxComponent();
		//			//	float kInvLogNat2 = 1.4426950408889634073599246810019f;
		//			//	numMips = 1 + (int)( Math.Log( size.MaxComponent() ) * kInvLogNat2 );
		//			//}
		//			//else
		//			//	numMips = 1;

		//			for( int face = 0; face < 6; face++ )
		//			{
		//				//!!!!
		//				var mip = 0;
		//				//for( int mip = 0; mip < numMips; mip++ )
		//				//{

		//				var renderTexture = CaptureTexture.Result.GetRenderTarget( mip, face );
		//				//!!!!
		//				var viewport = renderTexture.AddViewport( true, false );
		//				//var viewport = renderTexture.AddViewport( false, false );
		//				viewport.Mode = Viewport.ModeEnum.ReflectionProbeCubemap;
		//				viewport.AttachedScene = ParentScene;

		//				//!!!!���� �� ������� ���-��? ������ ������� ����� �� ������

		//				//}
		//				//!!!!���-�� ���?
		//			}

		//			//captureTextureNeedUpdate = true;
		//		}
		//	}
		//	else
		//		CaptureTextureDispose();

		//	//!!!!!��� ��������� ������ �������� �������? ����� ���
		//	//if( texture == null )
		//	//{
		//	//	//!!!!!
		//	//	Log.Fatal( "ViewportRenderingPipeline: RenderTarget_Alloc: Unable to create texture." );
		//	//	return null;
		//	//}

		//	//int faces = type == Component_Image.TypeEnum.Cube ? 6 : arrayLayers;

		//	//int numMips;
		//	//if( mipmaps )
		//	//{
		//	//	int max = size.MaxComponent();
		//	//	float kInvLogNat2 = 1.4426950408889634073599246810019f;
		//	//	numMips = 1 + (int)( Math.Log( size.MaxComponent() ) * kInvLogNat2 );
		//	//}
		//	//else
		//	//	numMips = 1;

		//	//for( int face = 0; face < faces; face++ )
		//	//{
		//	//	for( int mip = 0; mip < numMips; mip++ )
		//	//	{
		//	//		RenderTexture renderTexture = texture.Result.GetRenderTarget( mip, face );
		//	//		var viewport = renderTexture.AddViewport( false, false );

		//	//		viewport.RenderingPipelineCreate();
		//	//		viewport.RenderingPipelineCreated.UseRenderTargets = false;
		//	//	}
		//	//	//!!!!���-�� ���?
		//	//}
		//}

		//void CaptureTextureDispose()
		//{
		//	captureTexture?.Dispose();
		//	captureTexture = null;
		//}

		//[Browsable( false )]
		//public Component_Image CaptureTexture
		//{
		//	get { return captureTexture; }
		//}

		string GetDestVirtualFileName()
		{
			string name = GetPathFromRoot();
			foreach( char c in new string( Path.GetInvalidFileNameChars() ) + new string( Path.GetInvalidPathChars() ) )
				name = name.Replace( c.ToString(), "_" );
			name = name.Replace( " ", "_" );
			return Path.Combine( ComponentUtility.GetOwnedFileNameOfComponent( this ) + "_Files", name + ".hdr" );
		}

		public void UpdateCaptureCubemap()
		{
			if( Mode.Value != ModeEnum.Capture )
				return;

			Component_Image texture = null;
			Component_Image textureRead = null;

			try
			{
				//create

				var resolution = Resolution.Value;
				var hdr = HDR.Value;

				var size = int.Parse( resolution.ToString().Replace( "_", "" ) );
				//!!!!16 ��� ����������, ����� ����� ��������� ��� Image2D
				PixelFormat format = hdr ? PixelFormat.Float32RGBA : PixelFormat.A8R8G8B8;
				//PixelFormat format = hdr ? PixelFormat.Float16RGBA : PixelFormat.A8R8G8B8;

				texture = ComponentUtility.CreateComponent<Component_Image>( null, true, false );
				texture.CreateType = Component_Image.TypeEnum._2D;
				texture.CreateSize = new Vector2I( size, size );
				texture.CreateMipmaps = false;
				texture.CreateFormat = format;
				texture.CreateUsage = Component_Image.Usages.RenderTarget;
				texture.CreateFSAA = 0;
				texture.Enabled = true;

				var renderTexture = texture.Result.GetRenderTarget( 0, 0 );
				//!!!!
				var viewport = renderTexture.AddViewport( true, false );
				//var viewport = renderTexture.AddViewport( false, false );
				viewport.Mode = Viewport.ModeEnum.ReflectionProbeCubemap;
				viewport.AttachedScene = ParentScene;

				textureRead = ComponentUtility.CreateComponent<Component_Image>( null, true, false );
				textureRead.CreateType = Component_Image.TypeEnum._2D;
				textureRead.CreateSize = new Vector2I( size, size );
				textureRead.CreateMipmaps = false;
				textureRead.CreateFormat = format;
				textureRead.CreateUsage = Component_Image.Usages.ReadBack | Component_Image.Usages.BlitDestination;
				textureRead.CreateFSAA = 0;
				textureRead.Enabled = true;
				//!!!!
				textureRead.Result.PrepareNativeObject();

				//render
				var image2D = new ImageUtility.Image2D( PixelFormat.Float32RGB, new Vector2I( size * 4, size * 3 ) );

				var position = Transform.Value.Position;

				for( int face = 0; face < 6; face++ )
				{
					Vector3 dir = Vector3.Zero;
					Vector3 up = Vector3.Zero;

					//flipped
					switch( face )
					{
					case 0: dir = -Vector3.YAxis; up = Vector3.ZAxis; break;
					case 1: dir = Vector3.YAxis; up = Vector3.ZAxis; break;
					case 2: dir = Vector3.ZAxis; up = -Vector3.XAxis; break;
					case 3: dir = -Vector3.ZAxis; up = Vector3.XAxis; break;
					case 4: dir = Vector3.XAxis; up = Vector3.ZAxis; break;
					case 5: dir = -Vector3.XAxis; up = Vector3.ZAxis; break;
					}

					//!!!!renderingPipelineOverride

					var cameraSettings = new Viewport.CameraSettingsClass( viewport, 1, 90, NearClipPlane.Value, FarClipPlane.Value, position, dir, up, ProjectionType.Perspective, 1, 1, 1 );

					viewport.Update( true, cameraSettings );

					//clear temp data
					viewport.RenderingContext.MultiRenderTarget_DestroyAll();
					viewport.RenderingContext.DynamicTexture_DestroyAll();

					texture.Result.RealObject.BlitTo( viewport.RenderingContext.CurrentViewNumber, textureRead.Result.RealObject, 0, 0 );

					//get data
					var totalBytes = PixelFormatUtility.GetNumElemBytes( format ) * size * size;
					var data = new byte[ totalBytes ];
					unsafe
					{
						fixed ( byte* pBytes = data )
						{
							var demandedFrame = textureRead.Result.RealObject.Read( (IntPtr)pBytes, 0 );

							while( RenderingSystem.CallBgfxFrame() < demandedFrame )
							{
							}
						}
					}

					Vector2I index = Vector2I.Zero;
					switch( face )
					{
					case 0: index = new Vector2I( 2, 1 ); break;
					case 1: index = new Vector2I( 0, 1 ); break;
					case 2: index = new Vector2I( 1, 0 ); break;
					case 3: index = new Vector2I( 1, 2 ); break;
					case 4: index = new Vector2I( 1, 1 ); break;
					case 5: index = new Vector2I( 3, 1 ); break;
					}

					var faceImage = new ImageUtility.Image2D( format, new Vector2I( size, size ), data );
					image2D.Blit( index * size, faceImage );
				}

				var destRealFileName = VirtualPathUtility.GetRealPathByVirtual( GetDestVirtualFileName() );

				if( !Directory.Exists( Path.GetDirectoryName( destRealFileName ) ) )
					Directory.CreateDirectory( Path.GetDirectoryName( destRealFileName ) );

				if( !ImageUtility.Save( destRealFileName, image2D.Data, image2D.Size, 1, image2D.Format, 1, 0, out var error ) )
					throw new Exception( error );
			}
			catch( Exception e )
			{
				Log.Error( e.Message );
			}
			finally
			{
				texture?.Dispose();
				textureRead?.Dispose();
			}

			processedCubemapNeedUpdate = true;
		}

		//public void Update( bool forceUpdate )
		//{
		//	CaptureTextureUpdate();

		//	if( CaptureTexture != null && ( Realtime || captureTextureNeedUpdate || forceUpdate ) )
		//	{
		//		captureTextureNeedUpdate = false;

		//		var position = Transform.Value.Position;

		//		for( int face = 0; face < 6; face++ )
		//		{
		//			int mip = 0;
		//			var renderTexture = CaptureTexture.Result.GetRenderTarget( mip, face );
		//			if( renderTexture != null )
		//			{
		//				var viewport = renderTexture.Viewports[ 0 ];

		//				Vector3 dir = Vector3.Zero;
		//				Vector3 up = Vector3.Zero;

		//				//flipped
		//				switch( face )
		//				{
		//				case 0: dir = -Vector3.YAxis; up = Vector3.ZAxis; break;
		//				case 1: dir = Vector3.YAxis; up = Vector3.ZAxis; break;
		//				case 2: dir = Vector3.ZAxis; up = -Vector3.XAxis; break;
		//				case 3: dir = -Vector3.ZAxis; up = Vector3.XAxis; break;
		//				case 4: dir = Vector3.XAxis; up = Vector3.ZAxis; break;
		//				case 5: dir = -Vector3.XAxis; up = Vector3.ZAxis; break;
		//				}

		//				//!!!!
		//				var cameraSettings = new Viewport.CameraSettingsClass( viewport, 1, 90, NearClipPlane.Value, FarClipPlane.Value, position, dir, up, ProjectionType.Perspective, 1, 1, 1 );

		//				viewport.Update( true, cameraSettings );

		//				//clear temp data
		//				if( !Realtime.Value )
		//				{
		//					viewport.RenderingContext.MultiRenderTarget_DestroyAll();
		//					viewport.RenderingContext.RenderTarget_DestroyAll();
		//				}
		//			}
		//		}
		//	}
		//}

		void UpdateProcessedCubemaps()
		{
			if( processedEnvironmentCubemap != null && processedEnvironmentCubemap.Disposed )
			{
				processedEnvironmentCubemap = null;
				processedCubemapNeedUpdate = true;
			}
			if( processedIrradianceCubemap != null && processedIrradianceCubemap.Disposed )
			{
				processedIrradianceCubemap = null;
				processedCubemapNeedUpdate = true;
			}

			if( processedCubemapNeedUpdate )//&& AllowProcessEnvironmentCubemap )
			{
				processedCubemapNeedUpdate = false;
				ProcessCubemaps();
			}
		}

		void ProcessCubemaps()
		{
			processedEnvironmentCubemap = null;
			processedIrradianceCubemap = null;

			string sourceFileName;
			if( Mode.Value == ModeEnum.Resource )
			{
				sourceFileName = Cubemap.Value?.LoadFile.Value?.ResourceName;
				if( string.IsNullOrEmpty( sourceFileName ) )
				{
					var getByReference = Cubemap.GetByReference;
					if( !string.IsNullOrEmpty( getByReference ) )
					{
						try
						{
							if( Path.GetExtension( getByReference ) == ".image" )
								sourceFileName = getByReference;
						}
						catch { }
					}
				}
			}
			else
			{
				sourceFileName = GetDestVirtualFileName();
			}

			if( !string.IsNullOrEmpty( sourceFileName ) && VirtualFile.Exists( sourceFileName ) )
			{
				bool skip = false;
				if( sourceFileName.Length > 11 )
				{
					var s = sourceFileName.Substring( sourceFileName.Length - 11 );
					if( s == "_GenEnv.dds" || s == "_GenIrr.dds" )
						skip = true;
				}

				if( !skip )
				{
					if( !CubemapProcessing.GetOrGenerate( sourceFileName, false, 0, out var envVirtualFileName, out var irrVirtualFileName, out var error ) )
					{
						Log.Error( error );
						return;
					}

					if( VirtualFile.Exists( envVirtualFileName ) )
						processedEnvironmentCubemap = ResourceManager.LoadResource<Component_Image>( envVirtualFileName );
					if( VirtualFile.Exists( irrVirtualFileName ) )
						processedIrradianceCubemap = ResourceManager.LoadResource<Component_Image>( irrVirtualFileName );
				}
			}
		}
	}
}
