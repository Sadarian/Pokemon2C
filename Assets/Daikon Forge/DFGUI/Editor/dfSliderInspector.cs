﻿/* Copyright 2013 Daikon Forge */
using UnityEngine;
using UnityEditor;

using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor( typeof( dfSlider ) )]
public class dfSliderInspector : dfControlInspector
{

	private static Dictionary<int, bool> foldouts = new Dictionary<int, bool>();

	protected override bool OnCustomInspector()
	{

		var control = target as dfSlider;
		if( control == null )
			return false;

		dfEditorUtil.DrawSeparator();

		if( !isFoldoutExpanded( foldouts, "Slider Properties", true ) )
			return false;

		EditorGUIUtility.LookLikeControls( 100f );
		EditorGUI.indentLevel += 1;

		GUILayout.Label( "Appearance", "HeaderLabel" );
		{

			SelectTextureAtlas( "Atlas", control, "Atlas", false, true );
			SelectSprite( "Track", control.Atlas, control, "BackgroundSprite", false );

			var orientation = (dfControlOrientation)EditorGUILayout.EnumPopup( "Orientation", control.Orientation );
			if( orientation != control.Orientation )
			{
				dfEditorUtil.MarkUndo( control, "Change Orientation" );
				control.Orientation = orientation;
			}

		}

		GUILayout.Label( "Behavior", "HeaderLabel" );
		{

			var min = EditorGUILayout.FloatField( "Min Value", control.MinValue );
			if( min != control.MinValue )
			{
				dfEditorUtil.MarkUndo( control, "Change Minimum Value" );
				control.MinValue = min;
			}

			var max = EditorGUILayout.FloatField( "Max Value", control.MaxValue );
			if( max != control.MaxValue )
			{
				dfEditorUtil.MarkUndo( control, "Change Maximum Value" );
				control.MaxValue = max;
			}

			var step = EditorGUILayout.FloatField( "Step", control.StepSize );
			if( step != control.StepSize )
			{
				dfEditorUtil.MarkUndo( control, "Change Step" );
				control.StepSize = step;
			}

			var scroll = EditorGUILayout.FloatField( "Scroll Size", control.ScrollSize );
			if( scroll != control.ScrollSize )
			{
				dfEditorUtil.MarkUndo( control, "Change Scroll Increment" );
				control.ScrollSize = scroll;
			}

			var value = EditorGUILayout.Slider( "Value", control.Value, control.MinValue, control.MaxValue );
			if( value != control.Value )
			{
				dfEditorUtil.MarkUndo( control, "Change Slider Value" );
				control.Value = value;
			}

		}

		GUILayout.Label( "Controls", "HeaderLabel" );
		{

			var thumb = EditorGUILayout.ObjectField( "Thumb", control.Thumb, typeof( dfControl ), true ) as dfControl;
			if( thumb != control.Thumb )
			{
				dfEditorUtil.MarkUndo( control, "Assign Thumb Object" );
				control.Thumb = thumb;
			}

			if( thumb != null )
			{

				var thumbPadding = EditInt2( "Offset", "X", "Y", control.ThumbOffset );
				if( !RectOffset.Equals( thumbPadding, control.ThumbOffset ) )
				{
					dfEditorUtil.MarkUndo( control, "Change thumb Offset" );
					control.ThumbOffset = thumbPadding;
				}

			}

			var fill = EditorGUILayout.ObjectField( "Progress", control.Progress, typeof( dfControl ), true ) as dfControl;
			if( fill != control.Progress )
			{
				dfEditorUtil.MarkUndo( control, "Assign Thumb Object" );
				control.Progress = fill;
			}

			if( fill != null )
			{

				if( fill is dfSprite )
				{

					var mode = (dfProgressFillMode)EditorGUILayout.EnumPopup( "Fill Mode", control.FillMode );
					if( mode != control.FillMode )
					{
						dfEditorUtil.MarkUndo( control, "Change Fill Mode" );
						control.FillMode = mode;
					}

				}

				var padding = EditPadding( "Padding", control.FillPadding );
				if( padding != control.FillPadding )
				{
					dfEditorUtil.MarkUndo( control, "Change Slider Padding" );
					control.FillPadding = padding;
				}

			}

		}

		return true;

	}

}
