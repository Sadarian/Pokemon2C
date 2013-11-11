﻿/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Object = UnityEngine.Object;

[CustomEditor( typeof( dfAtlas ) )]
public class dfTextureAtlasInspector : Editor
{

	#region Public static properties

	public static string SelectedSprite { get; set; }

	#endregion

	#region Private fields

	private static Texture2D lineTex;

	#endregion

	#region Atlas creation

	[MenuItem( "Assets/Daikon Forge/Create Texture Atlas" )]
	//[MenuItem( "GameObject/Daikon Forge/Create Texture Atlas" )]
	public static void CreateAtlasFromSelection()
	{

		var selection = Selection
			.GetFiltered( typeof( Texture2D ), SelectionMode.Assets )
			.Cast<Texture2D>()
			.Where( t => isReadable( t ) )
			.OrderBy( t => t.name )
			.ToArray();

		if( selection.Length == 0 )
		{
			EditorUtility.DisplayDialog( "Create Texture Atlas", "Either no textures selected or none of the selected textures has Read/Write enabled", "OK" );
			return;
		}

		var saveFolder = Path.GetDirectoryName( AssetDatabase.GetAssetPath( selection[ 0 ] ) );
		var prefabPath = EditorUtility.SaveFilePanel( "Create Font Definition", saveFolder, "Texture Atlas", "prefab" );
		if( string.IsNullOrEmpty( prefabPath ) )
			return;

		prefabPath = prefabPath.MakeRelativePath();

		var texture = new Texture2D( 1, 1, TextureFormat.ARGB32, false );
		var rects = texture.PackTextures( selection, 2, 4096 );

		var texturePath = Path.ChangeExtension( prefabPath, "png" );
		byte[] bytes = texture.EncodeToPNG();
		System.IO.File.WriteAllBytes( texturePath, bytes );
		bytes = null;
		DestroyImmediate( texture );

		setTextureImportSettings( texturePath, FilterMode.Bilinear );

		texture = AssetDatabase.LoadAssetAtPath( texturePath, typeof( Texture2D ) ) as Texture2D;
		if( texture == null )
			Debug.LogError( "Failed to find texture at " + texturePath );

		var sprites = new List<dfAtlas.ItemInfo>();
		for( int i = 0; i < rects.Length; i++ )
		{

			var pixelCoords = rects[ i ];

			var item = new dfAtlas.ItemInfo()
			{
				name = selection[ i ].name,
				region = pixelCoords,
				rotated = false,
				texture = selection[ i ]
			};

			sprites.Add( item );

		}

		var shader = Shader.Find( "Daikon Forge/Default UI Shader" );
		var atlasMaterial = new Material( shader );
		atlasMaterial.mainTexture = texture;
		AssetDatabase.CreateAsset( atlasMaterial, Path.ChangeExtension( texturePath, "mat" ) );

		var go = new GameObject() { name = Path.GetFileNameWithoutExtension( prefabPath ) };
		var atlas = go.AddComponent<dfAtlas>();
		atlas.material = atlasMaterial;
		atlas.AddItems( sprites );

		var prefab = PrefabUtility.CreateEmptyPrefab( prefabPath );
		prefab.name = atlas.name;
		PrefabUtility.ReplacePrefab( go, prefab );

		DestroyImmediate( go );
		AssetDatabase.Refresh();

		#region Delay execution of object selection to work around a Unity issue

		// Declared with null value to eliminate "uninitialized variable" 
		// compiler error in lambda below.
		EditorApplication.CallbackFunction callback = null;

		callback = () =>
		{
			EditorUtility.FocusProjectWindow();
			go = AssetDatabase.LoadMainAssetAtPath( prefabPath ) as GameObject;
			Selection.objects = new Object[] { go };
			EditorGUIUtility.PingObject( go );
			Debug.Log( "Texture Atlas prefab created at " + prefabPath, prefab );
			EditorApplication.delayCall -= callback;
		};

		EditorApplication.delayCall += callback;

		#endregion

	}

	private void rebuildAtlas( dfAtlas atlas )
	{

		var textures = atlas.items.Select( i => i.texture ).ToList();

		var oldAtlasTexture = atlas.material.mainTexture;
		var texturePath = AssetDatabase.GetAssetPath( oldAtlasTexture );

		var newAtlasTexture = new Texture2D( 0, 0, TextureFormat.RGBA32, false );
		var newRects = newAtlasTexture.PackTextures( textures.ToArray(), 2 );

		byte[] bytes = newAtlasTexture.EncodeToPNG();
		System.IO.File.WriteAllBytes( texturePath, bytes );
		bytes = null;
		DestroyImmediate( newAtlasTexture );

		setTextureImportSettings( texturePath, oldAtlasTexture.filterMode );

		// Fix up the new sprite locations
		for( int i = 0; i < atlas.Count; i++ )
		{
			atlas.items[ i ].region = newRects[ i ];
		}

		// Re-sort the Items collection
		atlas.items.Sort();
		atlas.RebuildIndexes();

		EditorUtility.SetDirty( atlas );
		EditorUtility.SetDirty( atlas.material );

		dfGUIManager.RefreshAll();

	}

	public void RemoveSprite( dfAtlas atlas, string spriteName )
	{

		atlas.Remove( spriteName );

		var textures = atlas.items.Select( i => i.texture ).ToList();

		var oldAtlasTexture = atlas.material.mainTexture;
		var texturePath = AssetDatabase.GetAssetPath( oldAtlasTexture );

		var newAtlasTexture = new Texture2D( 0, 0, TextureFormat.RGBA32, false );
		var newRects = newAtlasTexture.PackTextures( textures.ToArray(), 2 );

		byte[] bytes = newAtlasTexture.EncodeToPNG();
		System.IO.File.WriteAllBytes( texturePath, bytes );
		bytes = null;
		DestroyImmediate( newAtlasTexture );

		setTextureImportSettings( texturePath, oldAtlasTexture.filterMode );

		// Fix up the new sprite locations
		for( int i = 0; i < atlas.Count; i++ )
		{
			atlas.items[ i ].region = newRects[ i ];
		}

		// Re-sort the Items collection
		atlas.items.Sort();
		atlas.RebuildIndexes();

		EditorUtility.SetDirty( atlas );
		EditorUtility.SetDirty( atlas.material );

		dfGUIManager.RefreshAll();

	}

	public void AddTexture( dfAtlas atlas, params Texture2D[] newTextures )
	{

		for( int i = 0; i < newTextures.Length; i++ )
		{

			// Grab reference to existing item, if it exists, to preserve border information
			var existingItem = atlas[ newTextures[ i ].name ];

			// Remove the existing item if it already exists
			atlas.Remove( newTextures[ i ].name );

			// Add the new texture to the Items collection
			atlas.AddItem( new dfAtlas.ItemInfo()
			{
				texture = newTextures[ i ],
				name = newTextures[ i ].name,
				border = ( existingItem != null ) ? existingItem.border : new RectOffset()
			} );

		}

		var textures = atlas.items.Select( i => i.texture ).ToList();

		var oldAtlasTexture = atlas.material.mainTexture;
		var texturePath = AssetDatabase.GetAssetPath( oldAtlasTexture );

		var newAtlasTexture = new Texture2D( 0, 0, TextureFormat.RGBA32, false );
		var newRects = newAtlasTexture.PackTextures( textures.ToArray(), 2 );

		byte[] bytes = newAtlasTexture.EncodeToPNG();
		System.IO.File.WriteAllBytes( texturePath, bytes );
		bytes = null;
		DestroyImmediate( newAtlasTexture );

		setTextureImportSettings( texturePath, oldAtlasTexture.filterMode );

		// Fix up the new sprite locations
		for( int i = 0; i < atlas.Count; i++ )
		{
			atlas.items[ i ].region = newRects[ i ];
		}

		// Re-sort the Items collection
		atlas.items.Sort();
		atlas.RebuildIndexes();

		EditorUtility.SetDirty( atlas );
		EditorUtility.SetDirty( atlas.material );

		dfGUIManager.RefreshAll();

	}

	private static bool isReadable( Texture2D texture )
	{

		var path = AssetDatabase.GetAssetPath( texture );
		var importer = AssetImporter.GetAtPath( path ) as TextureImporter;
		if( importer == null || !importer.isReadable )
		{
			return false;
		}

		return true;

	}

	private static void setTextureImportSettings( string path, FilterMode filterMode )
	{

		AssetDatabase.Refresh();
		var importer = AssetImporter.GetAtPath( path ) as TextureImporter;
		if( importer == null )
			Debug.LogError( "Failed to find importer" );

		var settings = new TextureImporterSettings();

		importer.ReadTextureSettings( settings );
		settings.mipmapEnabled = false;
		settings.readable = true;
		settings.maxTextureSize = 4096;
		settings.textureFormat = TextureImporterFormat.ARGB32;
		settings.filterMode = filterMode;
		settings.wrapMode = TextureWrapMode.Clamp;
		settings.npotScale = TextureImporterNPOTScale.None;
		settings.linearTexture = true;
		importer.SetTextureSettings( settings );

		AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate );

	}

	#endregion

	public override void OnInspectorGUI()
	{

		var atlas = target as dfAtlas;

		var atlasInfo = string.Format(
			"Texture Atlas: {0}\nSprites: {1}\nTexture: {2}\nFormat: {3}",
			atlas.name,
			atlas.items.Count,
			atlas.Texture != null ? string.Format( "{0}x{1}", atlas.Texture.width, atlas.Texture.height ) : "[none]",
			atlas.Texture.format
		);

		GUILayout.Label( atlasInfo );

		ShowAddTextureOption( atlas );
		ShowModifiedTextures( atlas );

		GUILayout.Label( "Edit Sprite", "HeaderLabel" );
		EditorGUIUtility.LookLikeControls( 94f );

		EditSprite( "Edit Sprite" );

		var sprite = atlas[ SelectedSprite ];
		if( sprite == null )
		{

			EditorGUILayout.BeginHorizontal();
			{

				if( GUILayout.Button( "Rebuild" ) )
				{
					rebuildAtlas( atlas );
				}

				if( GUILayout.Button( "Refresh Views" ) )
				{
					dfGUIManager.RefreshAll( true );
				}

			}
			EditorGUILayout.EndHorizontal();

			showSprites( atlas );

			return;

		}

		var spriteName = EditorGUILayout.TextField( "Name", sprite.name );
		if( spriteName != sprite.name )
		{
			dfEditorUtil.MarkUndo( target, "Change sprite name" );
			sprite.name = spriteName;
		}

		EditorGUILayout.BeginHorizontal();
		{

			if( GUILayout.Button( "Cancel" ) )
			{
				SelectedSprite = null;
			}

			if( GUILayout.Button( "Remove Sprite" ) )
			{
				if( EditorUtility.DisplayDialog( "Remove Sprite", "Are you sure you want to remove " + spriteName + " from the Atlas?", "Yes", "No" ) )
				{
					dfEditorUtil.MarkUndo( atlas, "Remove Sprite" );
					RemoveSprite( atlas, SelectedSprite );
					EditorUtility.DisplayDialog( "Sprite Removed", SelectedSprite + " has been removed", "Ok" );
					SelectedSprite = "";
					dfGUIManager.RefreshAll();
				}
			}

		}
		EditorGUILayout.EndHorizontal();

		if( sprite.texture == null )
			return;

		var location = new Vector2( sprite.region.x * sprite.texture.width, sprite.region.y * sprite.texture.height );
		var size = new Vector2( sprite.texture.width, sprite.texture.height );

		dfEditorUtil.DrawHorzLine();
		EditInt2( "Location", "Left", "Top", location, 90, false );

		dfEditorUtil.DrawHorzLine();
		EditInt2( "Size", "Width", "Height", size, 90, false );

		dfEditorUtil.DrawHorzLine();
		var borders = EditRectOffset( "Slices", "Left", "Top", "Right", "Bottom", sprite.border, 90 );
		if( !borders.Equals( sprite.border ) )
		{
			dfEditorUtil.MarkUndo( target, "Change sprite borders" );
			sprite.border = borders;
		}

		if( GUILayout.Button( "Refresh Views" ) )
		{
			dfGUIManager.RefreshAll( true );
		}

	}

	private void showSprites( dfAtlas atlas )
	{

		EditorGUIUtility.LookLikeControls( 100f );
		EditorGUI.indentLevel += 1;

		EditorGUILayout.Separator();

		GUILayout.Label( "Sprites", "HeaderLabel" );

		for( int i = 0; i < atlas.items.Count; i++ )
		{

			var sprite = atlas.items[ i ];

			dfEditorUtil.DrawSeparator();

			var removeSprite = false;
			EditorGUILayout.BeginHorizontal();
			{

				GUILayout.Label( sprite.name );

				if( GUILayout.Button( "Edit", GUILayout.Width( 75 ) ) )
				{
					SelectedSprite = sprite.name;
				}

				if( GUILayout.Button( "Delete", GUILayout.Width( 75 ) ) )
				{
					removeSprite = true;
				}

			}
			EditorGUILayout.EndHorizontal();

			if( removeSprite )
			{
				RemoveSprite( atlas, sprite.name );
				continue;
			}

			if( sprite.texture == null )
			{
				EditorGUILayout.HelpBox( "This sprite's texture has been deleted", MessageType.Error );
			}
			else
			{

				var size = 75; // Mathf.Min( 75, Mathf.Max( sprite.sizeInPixels.x, sprite.sizeInPixels.y ) );
				var rect = GUILayoutUtility.GetRect( size, size );
				drawSprite( rect, sprite );

			}

		}

		EditorGUI.indentLevel -= 1;

	}

	private void drawSprite( Rect rect, dfAtlas.ItemInfo sprite )
	{

		var texture = sprite.texture;
		if( texture == null )
			return;

		var size = new Vector2( texture.width, texture.height );
		var destRect = rect;

		if( destRect.width < size.x || destRect.height < size.y )
		{

			var newHeight = size.y * rect.width / size.x;
			if( newHeight <= rect.height )
				destRect.height = newHeight;
			else
				destRect.width = size.x * rect.height / size.y;

		}
		else
		{
			destRect.width = size.x;
			destRect.height = size.y;
		}

		if( destRect.width < rect.width ) destRect.x = rect.x + ( rect.width - destRect.width ) * 0.5f;
		if( destRect.height < rect.height ) destRect.y = rect.y + ( rect.height - destRect.height ) * 0.5f;

		GUI.DrawTexture( destRect, texture );

	}

	static public void DrawBox( Rect rect, Color color )
	{
		if( Event.current.type == EventType.Repaint )
		{
			Texture2D tex = EditorGUIUtility.whiteTexture;
			GUI.color = color;
			GUI.DrawTexture( rect, tex );
			GUI.color = Color.white;
		}
	}

	private void ShowModifiedTextures( dfAtlas atlas )
	{

		var atlasPath = AssetDatabase.GetAssetPath( atlas.Texture );
		var atlasModified = File.GetLastWriteTime( atlasPath );

		var modifiedSprites = new List<dfAtlas.ItemInfo>();

		for( int i = 0; i < atlas.items.Count; i++ )
		{

			var sprite = atlas.items[ i ];

			var spriteTexturePath = AssetDatabase.GetAssetPath( sprite.texture );
			if( string.IsNullOrEmpty( spriteTexturePath ) || !File.Exists( spriteTexturePath ) )
				continue;

			var spriteModified = File.GetLastWriteTime( spriteTexturePath );
			if( spriteModified > atlasModified )
			{
				modifiedSprites.Add( sprite );
			}

		}

		if( modifiedSprites.Count == 0 )
			return;

		GUILayout.Label( "Modified Sprites", "HeaderLabel" );

		var list = string.Join( "\n\t", modifiedSprites.Select( s => s.name ).ToArray() );
		var message = string.Format( "The following textures have been modified:\n\t{0}", list );

		EditorGUILayout.HelpBox( message, MessageType.Info );

		var performUpdate = GUILayout.Button( "Refresh Modified Sprites" );
		dfEditorUtil.DrawSeparator();

		if( !performUpdate )
		{
			return;
		}

		for( int i = 0; i < modifiedSprites.Count; i++ )
		{
			var sprite = modifiedSprites[ i ];
			var spriteTexturePath = AssetDatabase.GetAssetPath( sprite.texture );
			sprite.texture = AssetDatabase.LoadAssetAtPath( spriteTexturePath, typeof( Texture2D ) ) as Texture2D;
		}

		var textures = atlas.items.Select( i => i.texture ).ToArray();

		var oldAtlasTexture = atlas.material.mainTexture;
		var atlasTexturePath = AssetDatabase.GetAssetPath( oldAtlasTexture );

		var newAtlasTexture = new Texture2D( 0, 0, TextureFormat.RGBA32, false );
		var newRects = newAtlasTexture.PackTextures( textures, 2 );

		byte[] bytes = newAtlasTexture.EncodeToPNG();
		System.IO.File.WriteAllBytes( atlasTexturePath, bytes );
		bytes = null;
		DestroyImmediate( newAtlasTexture );

		setTextureImportSettings( atlasTexturePath, oldAtlasTexture.filterMode );

		// Fix up the new sprite locations
		for( int i = 0; i < atlas.Count; i++ )
		{
			atlas.items[ i ].region = newRects[ i ];
		}

		// Re-sort the Items collection
		atlas.items.Sort();
		atlas.RebuildIndexes();

		EditorUtility.SetDirty( atlas );
		EditorUtility.SetDirty( atlas.material );

		dfGUIManager.RefreshAll();

		EditorUtility.DisplayDialog( "Refresh Sprites", message, "OK" );

	}

	private void ShowAddTextureOption( dfAtlas atlas )
	{

		dfEditorUtil.DrawSeparator();

		GUILayout.Label( "Add Sprites", "HeaderLabel" );
		{

			EditorGUILayout.HelpBox( "You can drag and drop textures here to add them to the Texture Atlas", MessageType.Info );

			var evt = Event.current;
			if( evt != null )
			{
				Rect dropRect = GUILayoutUtility.GetLastRect();
				if( evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform )
				{
					if( dropRect.Contains( evt.mousePosition ) )
					{
						var draggedTexture = DragAndDrop.objectReferences.FirstOrDefault( x => x is Texture2D );
						DragAndDrop.visualMode = ( draggedTexture != null ) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.None;
						if( evt.type == EventType.DragPerform )
						{
							addSelectedTextures( atlas );
						}
						evt.Use();
					}
				}
			}

		}

		dfEditorUtil.DrawSeparator();

	}

	private void addSelectedTextures( dfAtlas atlas )
	{

		var textures = DragAndDrop.objectReferences.Where( x => x is Texture2D ).Cast<Texture2D>().ToList();
		var notReadable = textures.Where( x => !isReadable( x ) ).OrderBy( x => x.name ).Select( x => x.name ).ToArray();
		var readable = textures.Where( x => isReadable( x ) ).OrderBy( x => x.name ).ToArray();

		AddTexture( atlas, readable );

		var message = string.Format( "{0} texture(s) added.", readable.Length );
		if( notReadable.Length > 0 )
		{
			message += "\nThe following textures were not set to Read/Write and could not be added:\n\n\t";
			message += string.Join( "\n\t", notReadable );
		}

		EditorUtility.DisplayDialog( "Add Sprites", message, "OK" );

		SelectedSprite = ( readable.Length > 0 ) ? readable.First().name : "";

	}

	protected Vector2 EditInt2( string groupLabel, string label1, string label2, Vector2 value, int labelWidth = 95, bool enabled = true )
	{

		try
		{

			var retVal = Vector2.zero;

			GUILayout.BeginHorizontal();
			{

				GUILayout.Label( groupLabel, GUILayout.Width( labelWidth ) );

				GUI.enabled = enabled;

				GUILayout.BeginVertical();
				{

					EditorGUIUtility.LookLikeControls( 50f );

					var x = EditorGUILayout.IntField( label1, (int)Math.Truncate( value.x ) );
					var y = EditorGUILayout.IntField( label2, (int)Math.Truncate( value.y ) );

					retVal.x = x;
					retVal.y = y;

				}
				GUILayout.EndVertical();

				GUILayout.FlexibleSpace();

			}
			GUILayout.EndHorizontal();

			EditorGUIUtility.LookLikeControls( 100f );

			return retVal;

		}
		finally
		{
			GUI.enabled = true;
		}

	}

	protected RectOffset EditRectOffset( string groupLabel, string leftLabel, string topLabel, string rightLabel, string bottomLabel, RectOffset value, int labelWidth = 95 )
	{

		EditorGUI.BeginChangeCheck();

		var retVal = new RectOffset();

		GUILayout.BeginHorizontal();
		{

			GUILayout.Label( groupLabel, GUILayout.Width( labelWidth ) );

			GUILayout.BeginVertical();
			{

				EditorGUIUtility.LookLikeControls( 50f );

				retVal.left = Mathf.Max( 0, EditorGUILayout.IntField( leftLabel, value != null ? value.left : 0 ) );
				retVal.right = Mathf.Max( 0, EditorGUILayout.IntField( rightLabel, value != null ? value.right : 0 ) );
				retVal.top = Mathf.Max( 0, EditorGUILayout.IntField( topLabel, value != null ? value.top : 0 ) );
				retVal.bottom = Mathf.Max( 0, EditorGUILayout.IntField( bottomLabel, value != null ? value.bottom : 0 ) );

			}
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

		}
		GUILayout.EndHorizontal();

		EditorGUIUtility.LookLikeControls( 100f );

		if( EditorGUI.EndChangeCheck() )
			return retVal;
		else
			return value;

	}

	protected internal void EditSprite( string label, int labelWidth = 90 )
	{

		var atlas = target as dfAtlas;
		if( atlas == null )
			return;

		dfSpriteSelectionDialog.SelectionCallback callback = delegate( string spriteName )
		{
			EditorUtility.SetDirty( target );
			SelectedSprite = spriteName;
		};

		var value = SelectedSprite;

		EditorGUILayout.BeginHorizontal();
		{

			GUILayout.Label( label, GUILayout.Width( labelWidth ) );

			var displayText = string.IsNullOrEmpty( value ) ? "[none selected]" : value;
			GUILayout.Label( displayText, "TextField", GUILayout.ExpandWidth( true ) );

			if( GUILayout.Button( new GUIContent( " ", "Edit " + label ), "IN ObjectField", GUILayout.Width( 12 ) ) )
			{
				dfSpriteSelectionDialog.Show( "Select Sprite: " + label, atlas, value, callback );
			}

		}
		EditorGUILayout.EndHorizontal();

		GUILayout.Space( 2 );

	}

	public override bool HasPreviewGUI()
	{

		var atlas = target as dfAtlas;

		return
			atlas != null &&
			atlas.Texture != null;

	}

	public override void OnPreviewGUI( Rect rect, GUIStyle background )
	{

		var atlas = target as dfAtlas;
		if( atlas == null )
			return;

		var sprite = atlas[ SelectedSprite ];
		if( sprite != null )
			previewSprite( rect );
		else
			previewAtlasTexture( atlas, rect );

		var texture = atlas.Texture;
		string text = string.Format( "Atlas Size: {0}x{1}", texture.width, texture.height );
		EditorGUI.DropShadowLabel( GUILayoutUtility.GetRect( Screen.width, 18f ), text );

	}

	internal static bool DrawAtlasPreview( GameObject item, Rect rect )
	{

		if( item == null )
			return false;

		var atlas = item.GetComponent<dfAtlas>();
		if( atlas == null )
			return false;

		previewAtlasTexture( atlas, rect );

		return true;

	}

	private static void previewAtlasTexture( dfAtlas atlas, Rect rect )
	{

		if( atlas == null )
			return;

		var texture = atlas.Texture;
		if( texture == null )
			return;

		var size = new Vector2( texture.width, texture.height );

		var destRect = rect;

		if( destRect.width < size.x || destRect.height < size.y )
		{

			var newHeight = size.y * rect.width / size.x;
			if( newHeight <= rect.height )
				destRect.height = newHeight;
			else
				destRect.width = size.x * rect.height / size.y;

		}
		else
		{
			destRect.width = size.x;
			destRect.height = size.y;
		}

		if( destRect.width < rect.width ) destRect.x = rect.x + ( rect.width - destRect.width ) * 0.5f;
		if( destRect.height < rect.height ) destRect.y = rect.y + ( rect.height - destRect.height ) * 0.5f;

		GUI.DrawTexture( destRect, texture );

	}

	private void previewSprite( Rect rect )
	{

		var atlas = target as dfAtlas;
		if( atlas == null )
			return;

		var spriteInfo = atlas[ SelectedSprite ];
		var texture = spriteInfo.texture;
		if( texture == null )
			return;

		var size = new Vector2( texture.width, texture.height );

		var destRect = rect;

		if( destRect.width < size.x || destRect.height < size.y )
		{

			var newHeight = size.y * rect.width / size.x;
			if( newHeight <= rect.height )
				destRect.height = newHeight;
			else
				destRect.width = size.x * rect.height / size.y;

		}
		else
		{
			destRect.width = size.x;
			destRect.height = size.y;
		}

		if( destRect.width < rect.width ) destRect.x = rect.x + ( rect.width - destRect.width ) * 0.5f;
		if( destRect.height < rect.height ) destRect.y = rect.y + ( rect.height - destRect.height ) * 0.5f;

		GUI.DrawTexture( destRect, texture );

		var border = spriteInfo.border;
		if( border.horizontal > 0 || border.vertical > 0 )
		{

			var lineColor = Color.white;
			lineColor.a = 0.7f;

			var left = Mathf.Floor( destRect.x + border.left * ( destRect.width / size.x ) );
			DrawLine( left, rect.y, rect.height, true, lineColor );

			var right = Mathf.Ceil( destRect.x + destRect.width - border.right * ( destRect.width / size.x ) );
			DrawLine( right, rect.y, rect.height, true, lineColor );

			var top = Mathf.Floor( destRect.y + border.top * ( destRect.height / size.y ) );
			DrawLine( rect.x, top, rect.width, false, lineColor );

			var bottom = Mathf.Ceil( destRect.y + destRect.height - border.bottom * ( destRect.height / size.y ) );
			DrawLine( rect.x, bottom, rect.width, false, lineColor );

		}

		string text = string.Format( "Sprite Size: {0}x{1}", size.x, size.y );
		EditorGUI.DropShadowLabel( GUILayoutUtility.GetRect( Screen.width, 18f ), text );

	}

	private void DrawLine( float left, float top, float size, bool vert, Color color )
	{

		if( !lineTex )
		{
			lineTex = new Texture2D( 1, 1 ) { hideFlags = HideFlags.HideAndDontSave };
		}

		var saveColor = GUI.color;
		GUI.color = color;

		if( !vert )
			GUI.DrawTexture( new Rect( left, top, size, 1 ), lineTex );
		else
			GUI.DrawTexture( new Rect( left, top, 1, size ), lineTex );

		GUI.color = saveColor;

	}

}
