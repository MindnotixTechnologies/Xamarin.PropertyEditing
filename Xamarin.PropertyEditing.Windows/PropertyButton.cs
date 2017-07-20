﻿using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using Xamarin.PropertyEditing.ViewModels;

namespace Xamarin.PropertyEditing.Windows
{
	internal class PropertyButton
		: Control
	{
		static PropertyButton ()
		{
			FocusableProperty.OverrideMetadata (typeof (PropertyButton), new FrameworkPropertyMetadata (false));
			DefaultStyleKeyProperty.OverrideMetadata (typeof(PropertyButton), new FrameworkPropertyMetadata (typeof(PropertyButton)));
		}

		public static readonly DependencyProperty CanSetCustomExpressionProperty = DependencyProperty.Register (
			"CanSetCustomExpression", typeof(bool), typeof(PropertyEditorControl), new PropertyMetadata (default(bool)));

		public bool CanSetCustomExpression
		{
			get { return (bool)GetValue (CanSetCustomExpressionProperty); }
			set { SetValue (CanSetCustomExpressionProperty, value); }
		}

		public static readonly DependencyProperty SystemResourcesSourceProperty = DependencyProperty.Register (
			"SystemResourcesSource", typeof (IEnumerable), typeof (PropertyEditorControl), new PropertyMetadata (default (IEnumerable)));

		public IEnumerable SystemResourcesSource
		{
			get { return (IEnumerable)GetValue (SystemResourcesSourceProperty); }
			set { SetValue (SystemResourcesSourceProperty, value); }
		}

		public static readonly DependencyProperty SystemResourceNamePathProperty = DependencyProperty.Register (
			"SystemResourceNamePath", typeof (string), typeof (PropertyEditorControl), new PropertyMetadata ("Name"));

		public string SystemResourceNamePath
		{
			get { return (string)GetValue (SystemResourceNamePathProperty); }
			set { SetValue (SystemResourceNamePathProperty, value); }
		}

		public static readonly DependencyProperty ValueSourceProperty = DependencyProperty.Register (
			"ValueSource", typeof(ValueSource), typeof(PropertyButton), new PropertyMetadata (ValueSource.Default, (o, args) => ((PropertyButton) o).OnValueSourceChanged ((ValueSource)args.NewValue)));

		public ValueSource ValueSource
		{
			get { return (ValueSource) GetValue (ValueSourceProperty); }
			set { SetValue (ValueSourceProperty, value); }
		}

		public static readonly DependencyProperty MenuTemplateProperty = DependencyProperty.Register (
			"MenuTemplate", typeof(DataTemplate), typeof(PropertyButton), new PropertyMetadata (default(DataTemplate)));

		public DataTemplate MenuTemplate
		{
			get { return (DataTemplate) GetValue (MenuTemplateProperty); }
			set { SetValue (MenuTemplateProperty, value); }
		}

		public override void OnApplyTemplate ()
		{
			base.OnApplyTemplate ();

			this.border = (Border) GetTemplateChild ("Border");
			this.indicator = (Rectangle) GetTemplateChild ("Indicator");

			this.border.MouseDown += OnBorderMouseDown;

			OnValueSourceChanged (ValueSource);
		}

		private Border border;
		private Rectangle indicator;
		private ContextMenu menu;

		private MenuItem systemResources;

		private const double MenuOffset = 3;

		private void OnBorderMouseDown (object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton != MouseButton.Left)
				return;

			if (this.menu == null) {
				this.menu = MenuTemplate?.LoadContent() as ContextMenu;
				if (this.menu == null)
					return;

				this.menu.PlacementTarget = this.indicator;
				this.menu.DataContext = DataContext;
			}

			this.menu.IsOpen = true;
			e.Handled = true;
		}

		private void OnValueSourceChanged (ValueSource source)
		{
			if (this.indicator == null)
				return;

			string brush = null;

			switch (source) {
				case ValueSource.Local:
					brush = "PropertyLocalValueBrush";
					break;
				case ValueSource.Binding:
					brush = "PropertyBoundValueBrush";
					break;
				case ValueSource.Inherited:
				case ValueSource.DefaultStyle:
				case ValueSource.Style:
				case ValueSource.Resource:
					brush = "PropertyResourceBrush";
					break;

				case ValueSource.Default:
					this.indicator.ClearValue (Shape.FillProperty);
					return;
			}

			this.indicator.SetResourceReference (Shape.FillProperty, brush);
		}
	}
}
