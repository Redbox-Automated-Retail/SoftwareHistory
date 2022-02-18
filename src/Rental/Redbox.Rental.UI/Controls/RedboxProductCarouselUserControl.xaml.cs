using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class RedboxProductCarouselUserControl : BaseUserControl, IComponentConnector
{
	private class CarouselPositionList : List<CarouselPosition>
	{
		public int CenterCarouselPositionIndex => NumberOfPositionsLeftOfCenter;

		public CarouselPosition CenterCarouselPosition => base[NumberOfPositionsLeftOfCenter];

		private int NumberOfPositionsLeftOfCenter => base.Count / 2;

		public int NextCenterCarouselPositionIndex(CarouselDirection direction)
		{
			return NumberOfPositionsLeftOfCenter + ((direction == CarouselDirection.Left) ? 1 : (-1));
		}
	}

	private class CarouselPosition
	{
		public double Width { get; set; }

		public double Height { get; set; }

		public double HorizontalCenter { get; set; }

		public double VerticalCenter { get; set; }

		public double Left => HorizontalCenter - Width / 2.0;

		public double Top => VerticalCenter - Height / 2.0;
	}

	private enum CarouselDirection
	{
		Left,
		Right
	}

	private const double ROTATION_DELAY = 2.0;

	public static readonly DependencyProperty IsRotatingProperty = DependencyProperty.Register("IsRotating", typeof(bool), typeof(RedboxProductCarouselUserControl), new FrameworkPropertyMetadata(false));

	private double originalHorizontalCenterOfCenterImage = 512.0;

	private int _carouselPositionCount;

	private int _carouselImagePadding;

	private double _horizontalCenterOfCenterImage;

	private double _verticalCenterOfCenterImage;

	private CarouselDirection _carouselDirection = CarouselDirection.Right;

	private CarouselDirection _nextCarouselDirection = CarouselDirection.Right;

	private Storyboard _carouselRotationStoryboard;

	private Point _carouselImageSmallSize;

	private Point _carouselImageLargeSize;

	private CarouselPositionList _carouselPositions = new CarouselPositionList();

	private CarouselTestModel _carouselModel;

	private List<DisplayProductModel> _carouselProducts;

	private List<DisplayProductModel> _rotatingCarouselProducts;

	private List<DisplayProductCarouselUserControl> _displayProductUserControlList = new List<DisplayProductCarouselUserControl>();

	private bool _forceRotation;

	private DispatcherTimer _timer;

	private bool _timerStarted;

	private bool _isStartViewAdABTest;

	public bool IsRotating
	{
		get
		{
			return (bool)GetValue(IsRotatingProperty);
		}
		set
		{
			SetValue(IsRotatingProperty, value);
		}
	}

	private bool IsCarouselProperlyInitialized
	{
		get
		{
			if (_rotatingCarouselProducts != null && _rotatingCarouselProducts.Count > 0 && _carouselPositions != null && _carouselPositions.Count > 0 && _displayProductUserControlList != null)
			{
				return _displayProductUserControlList.Count == _carouselPositions.Count;
			}
			return false;
		}
	}

	public RedboxProductCarouselUserControl()
	{
		InitializeComponent();
		ConfigureTimer();
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		ConfigureCarousel();
	}

	private void ConfigureCarousel()
	{
		_displayProductUserControlList.Clear();
		CarouselContainer.Children.Clear();
		_rotatingCarouselProducts = new List<DisplayProductModel>();
		InitializeCarouselPositions();
		_carouselModel = base.DataContext as CarouselTestModel;
		if (_carouselModel == null)
		{
			return;
		}
		_carouselModel.OnIsAnimationOnChanged += _carouselModel_OnIsAnimationOnChanged;
		_carouselProducts = _carouselModel.CarouselDisplayProductModels;
		_rotatingCarouselProducts.AddRange(_carouselProducts);
		_nextCarouselDirection = CarouselDirection.Right;
		foreach (DisplayProductModel rotatingCarouselProduct in _rotatingCarouselProducts)
		{
			int num = _rotatingCarouselProducts.IndexOf(rotatingCarouselProduct);
			if (num < _carouselPositions.Count)
			{
				CarouselPosition carouselPosition = _carouselPositions[num];
				DisplayProductCarouselUserControl displayProductCarouselUserControl = new DisplayProductCarouselUserControl
				{
					DataContext = rotatingCarouselProduct,
					Margin = new Thickness(carouselPosition.Left, carouselPosition.Top, 0.0, 0.0),
					Width = carouselPosition.Width,
					Height = carouselPosition.Height,
					BorderThickness = new Thickness(0.0),
					ShaderOpacity = 0.5,
					OpacityMaskStartX = 1.0
				};
				if (num < _carouselPositions.CenterCarouselPositionIndex)
				{
					displayProductCarouselUserControl.Tag = CarouselDirection.Right;
					Panel.SetZIndex(displayProductCarouselUserControl, num);
				}
				else if (num > _carouselPositions.CenterCarouselPositionIndex)
				{
					displayProductCarouselUserControl.Tag = CarouselDirection.Left;
					Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count - num);
				}
				else
				{
					Panel.SetZIndex(displayProductCarouselUserControl, _carouselPositions.Count);
					displayProductCarouselUserControl.ShaderOpacity = 0.0;
					displayProductCarouselUserControl.ShadowDepth = 10.0;
					displayProductCarouselUserControl.ShadowOpacity = 0.75;
				}
				_displayProductUserControlList.Add(displayProductCarouselUserControl);
				CarouselContainer.Children.Add(displayProductCarouselUserControl);
			}
		}
		CommandManager.InvalidateRequerySuggested();
		StartTimer();
	}

	private void _carouselModel_OnIsAnimationOnChanged(CarouselTestModel carouselModel)
	{
		if (carouselModel != null && carouselModel.IsAnimationOn)
		{
			StartTimer();
			return;
		}
		_carouselRotationStoryboard?.Stop();
		StopTimer();
		if (IsCarouselProperlyInitialized)
		{
			DisplayProductCarouselUserControl displayProductCarouselUserControl = _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex];
			if (displayProductCarouselUserControl != null && displayProductCarouselUserControl.DataContext is DisplayProductModel displayProductModel)
			{
				displayProductModel.AddButtonVisibility = Visibility.Collapsed;
			}
		}
	}

	private void AnimateCarouselRotation()
	{
		_carouselDirection = _nextCarouselDirection;
		TimeSpan timeSpan = TimeSpan.FromSeconds(0.0);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(_forceRotation ? 0 : 0);
		TimeSpan timeSpan3 = timeSpan + timeSpan2;
		TimeSpan timeSpan4 = TimeSpan.FromSeconds(0.0);
		TimeSpan timeSpan5 = timeSpan3 + timeSpan4;
		TimeSpan timeSpan6 = TimeSpan.FromSeconds(0.4);
		TimeSpan timeSpan7 = timeSpan5 + timeSpan6;
		TimeSpan timeSpan8 = timeSpan5 + TimeSpan.FromMilliseconds(timeSpan6.TotalMilliseconds / 2.0);
		TimeSpan timeSpan9 = timeSpan7;
		_forceRotation = false;
		_carouselRotationStoryboard?.Stop();
		_carouselRotationStoryboard = new Storyboard();
		_carouselRotationStoryboard.Duration = timeSpan9;
		_carouselRotationStoryboard.Completed += carousel_animation_storyBoard_Completed;
		if (IsCarouselProperlyInitialized)
		{
			for (int i = 0; i < _carouselPositionCount; i++)
			{
				if ((_carouselDirection != CarouselDirection.Left || i <= 0) && (_carouselDirection != CarouselDirection.Right || i >= _carouselPositionCount - 1))
				{
					continue;
				}
				CarouselPosition carouselPosition = _carouselPositions[i];
				int index = i + ((_carouselDirection != CarouselDirection.Left) ? 1 : (-1));
				CarouselPosition carouselPosition2 = _carouselPositions[index];
				DisplayProductCarouselUserControl displayProductCarouselUserControl = _displayProductUserControlList[i];
				_ = displayProductCarouselUserControl.DataContext;
				TranslateTransform value = new TranslateTransform();
				ScaleTransform value2 = new ScaleTransform();
				displayProductCarouselUserControl.RenderTransform = new TransformGroup
				{
					Children = 
					{
						(Transform)value2,
						(Transform)value
					}
				};
				bool flag = carouselPosition2 == _carouselPositions.CenterCarouselPosition;
				bool flag2 = carouselPosition == _carouselPositions.CenterCarouselPosition;
				double width = carouselPosition.Width;
				double value3 = width / carouselPosition.Width;
				double value4 = carouselPosition2.Width / width;
				DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames
				{
					KeyFrames = { (DoubleKeyFrame)new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = value3
					} }
				};
				if (flag2)
				{
					doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan),
						Value = 1.0
					});
					doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan3),
						Value = 1.0
					});
				}
				doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan5),
					Value = value3
				});
				doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = value4
				});
				Storyboard.SetTarget(doubleAnimationUsingKeyFrames, displayProductCarouselUserControl);
				Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleX)"));
				_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames);
				double height = carouselPosition.Height;
				double value5 = height / carouselPosition.Height;
				double value6 = carouselPosition2.Height / height;
				DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames
				{
					KeyFrames = { (DoubleKeyFrame)new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = value5
					} }
				};
				if (flag2)
				{
					doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan),
						Value = 1.0
					});
					doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan3),
						Value = 1.0
					});
				}
				doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan5),
					Value = value5
				});
				doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = value6
				});
				Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, displayProductCarouselUserControl);
				Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));
				_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames2);
				double value7 = carouselPosition2.HorizontalCenter - carouselPosition.HorizontalCenter;
				DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames
				{
					KeyFrames = 
					{
						(DoubleKeyFrame)new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 0.0
						},
						(DoubleKeyFrame)new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.0
						},
						(DoubleKeyFrame)new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = value7,
							EasingFunction = new CubicEase
							{
								EasingMode = EasingMode.EaseIn
							}
						}
					}
				};
				Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, displayProductCarouselUserControl);
				Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));
				_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames3);
				double value8 = carouselPosition2.VerticalCenter - carouselPosition.VerticalCenter;
				DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
				doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
					Value = 0.0
				});
				doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan5),
					Value = 0.0
				});
				doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = value8
				});
				Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, displayProductCarouselUserControl);
				Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));
				_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames4);
				if (flag2)
				{
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames5 = new DoubleAnimationUsingKeyFrames();
					if (_carouselDirection == CarouselDirection.Left)
					{
						displayProductCarouselUserControl.ConfigureForLeftMove();
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 1.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 1.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan8),
							Value = 0.6
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 1.0
						});
					}
					else
					{
						displayProductCarouselUserControl.ConfigureForRightMove();
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan5),
							Value = 0.0
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan8),
							Value = 0.4
						});
						doubleAnimationUsingKeyFrames5.KeyFrames.Add(new EasingDoubleKeyFrame
						{
							KeyTime = KeyTime.FromTimeSpan(timeSpan7),
							Value = 0.0
						});
					}
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames5, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames5, new PropertyPath("OpacityMaskStartX"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames5);
				}
				if (flag2)
				{
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames6 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames6.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = 0.5
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames6, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames6, new PropertyPath("ShaderOpacity"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames6);
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames7 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 10.0
					});
					doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = 10.0
					});
					doubleAnimationUsingKeyFrames7.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = 0.0
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames7, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames7, new PropertyPath("ShadowDepth"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames7);
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames8 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.75
					});
					doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = 0.75
					});
					doubleAnimationUsingKeyFrames8.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = 0.0
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames8, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames8, new PropertyPath("ShadowOpacity"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames8);
				}
				else if (flag)
				{
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames9 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.5
					});
					doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = 0.5
					});
					doubleAnimationUsingKeyFrames9.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = 0.0
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames9, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames9, new PropertyPath("ShaderOpacity"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames9);
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames10 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames10.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = 10.0
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames10, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames10, new PropertyPath("ShadowDepth"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames10);
					DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames11 = new DoubleAnimationUsingKeyFrames();
					doubleAnimationUsingKeyFrames11.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames11.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan5),
						Value = 0.0
					});
					doubleAnimationUsingKeyFrames11.KeyFrames.Add(new EasingDoubleKeyFrame
					{
						KeyTime = KeyTime.FromTimeSpan(timeSpan7),
						Value = 0.75
					});
					Storyboard.SetTarget(doubleAnimationUsingKeyFrames11, displayProductCarouselUserControl);
					Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames11, new PropertyPath("ShadowOpacity"));
					_carouselRotationStoryboard.Children.Add(doubleAnimationUsingKeyFrames11);
				}
				Int32AnimationUsingKeyFrames int32AnimationUsingKeyFrames = new Int32AnimationUsingKeyFrames();
				int zIndex = Panel.GetZIndex(displayProductCarouselUserControl);
				int num = ((_carouselDirection == CarouselDirection.Left) ? (i - 1) : (i + 1));
				int value9 = ((num < _carouselPositions.CenterCarouselPositionIndex) ? num : ((num <= _carouselPositions.CenterCarouselPositionIndex) ? _carouselPositions.Count : (_carouselPositions.Count - num)));
				int32AnimationUsingKeyFrames.KeyFrames.Add(new DiscreteInt32KeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan8),
					Value = value9
				});
				int32AnimationUsingKeyFrames.KeyFrames.Add(new DiscreteInt32KeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = zIndex
				});
				Storyboard.SetTarget(int32AnimationUsingKeyFrames, displayProductCarouselUserControl);
				Storyboard.SetTargetProperty(int32AnimationUsingKeyFrames, new PropertyPath("(Panel.ZIndex)"));
				_carouselRotationStoryboard.Children.Add(int32AnimationUsingKeyFrames);
				BooleanAnimationUsingKeyFrames booleanAnimationUsingKeyFrames = new BooleanAnimationUsingKeyFrames();
				booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
					Value = false
				});
				booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan3),
					Value = true
				});
				booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = true
				});
				Storyboard.SetTarget(booleanAnimationUsingKeyFrames, this);
				Storyboard.SetTargetProperty(booleanAnimationUsingKeyFrames, new PropertyPath("(local:RedboxProductCarouselUserControl.IsRotating)"));
				_carouselRotationStoryboard.Children.Add(booleanAnimationUsingKeyFrames);
			}
		}
		_carouselRotationStoryboard.Begin();
	}

	private void carousel_animation_storyBoard_Completed(object sender, EventArgs e)
	{
		if (_carouselModel.IsAnimationOn)
		{
			_carouselRotationStoryboard?.Stop();
			_carouselRotationStoryboard = null;
			RotateCarouselPositions();
			IsRotating = false;
			StartTimer();
		}
	}

	private void StartTimer()
	{
		if (_carouselModel.IsAnimationOn && !_timerStarted)
		{
			_timerStarted = true;
			_timer.Start();
		}
	}

	private void StopTimer()
	{
		_timer.Stop();
		_timerStarted = false;
	}

	private void ConfigureTimer()
	{
		_timer = new DispatcherTimer(DispatcherPriority.Send);
		_timer.Tick += _timer_Tick;
		_timer.Interval = TimeSpan.FromSeconds(2.0);
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		StopTimer();
		AnimateCarouselRotation();
	}

	private void RotateCarouselPositions()
	{
		if (!IsCarouselProperlyInitialized)
		{
			return;
		}
		List<DisplayProductModel> list = new List<DisplayProductModel>();
		int count = ((_carouselDirection == CarouselDirection.Left) ? 1 : (_rotatingCarouselProducts.Count - 1));
		list.AddRange(_rotatingCarouselProducts.Skip(count));
		list.AddRange(_rotatingCarouselProducts.Take(count));
		_rotatingCarouselProducts = list;
		foreach (DisplayProductCarouselUserControl displayProductUserControl in _displayProductUserControlList)
		{
			int num = _displayProductUserControlList.IndexOf(displayProductUserControl);
			displayProductUserControl.RenderTransform = null;
			displayProductUserControl.DataContext = _rotatingCarouselProducts[num];
			displayProductUserControl.ShaderOpacity = 0.5;
			if (num < _carouselPositions.CenterCarouselPositionIndex)
			{
				Panel.SetZIndex(displayProductUserControl, num);
				continue;
			}
			if (num > _carouselPositions.CenterCarouselPositionIndex)
			{
				Panel.SetZIndex(displayProductUserControl, _carouselPositions.Count - num);
				continue;
			}
			Panel.SetZIndex(displayProductUserControl, _carouselPositions.Count);
			displayProductUserControl.ShaderOpacity = 0.0;
			displayProductUserControl.ConfigureForLeftMove();
			displayProductUserControl.ShadowDepth = 10.0;
			displayProductUserControl.ShadowOpacity = 0.75;
		}
		_carouselModel.ProcessCarouselRotation(_rotatingCarouselProducts[_carouselPositions.CenterCarouselPositionIndex], null);
	}

	private void InitializeCarouselPositions()
	{
		InitializeCarouselImageSizes();
		_carouselPositions.Clear();
		int num = _carouselPositionCount / 2;
		_horizontalCenterOfCenterImage = originalHorizontalCenterOfCenterImage;
		double num2 = _horizontalCenterOfCenterImage - (_carouselImageLargeSize.X / 2.0 + (double)num * ((double)_carouselImagePadding + _carouselImageSmallSize.X) - _carouselImageSmallSize.X / 2.0);
		_ = _verticalCenterOfCenterImage;
		for (int i = 0; i < _carouselPositionCount; i++)
		{
			bool flag = i == _carouselPositionCount / 2;
			CarouselPosition carouselPosition = new CarouselPosition();
			carouselPosition.Width = (flag ? _carouselImageLargeSize.X : _carouselImageSmallSize.X);
			carouselPosition.Height = (flag ? _carouselImageLargeSize.Y : _carouselImageSmallSize.Y);
			carouselPosition.VerticalCenter = _verticalCenterOfCenterImage;
			carouselPosition.HorizontalCenter = num2 + _carouselPositions.Sum((CarouselPosition x) => x.Width) + (double)(_carouselPositions.Count() * _carouselImagePadding) - _carouselImageSmallSize.X / 2.0 + carouselPosition.Width / 2.0;
			_carouselPositions.Add(carouselPosition);
		}
	}

	private void InitializeCarouselImageSizes()
	{
		_isStartViewAdABTest = (base.DataContext as CarouselTestModel)?.IsStartViewAdABTest ?? false;
		_carouselImageSmallSize = (_isStartViewAdABTest ? new Point(232.0, 348.0) : new Point(303.0, 454.0));
		_carouselImageLargeSize = (_isStartViewAdABTest ? new Point(262.0, 393.0) : new Point(334.0, 501.0));
		_carouselPositionCount = (_isStartViewAdABTest ? 9 : 7);
		_carouselImagePadding = (_isStartViewAdABTest ? (-77) : (-100));
		_verticalCenterOfCenterImage = (_isStartViewAdABTest ? 161 : 210);
	}

	private void DisplayProductSelectedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		if (!IsRotating && IsCarouselProperlyInitialized)
		{
			DisplayProductCarouselUserControl displayProductUserControl = GetDisplayProductUserControl(e.OriginalSource);
			if (displayProductUserControl != null)
			{
				if (displayProductUserControl == _displayProductUserControlList[_carouselPositions.CenterCarouselPositionIndex])
				{
					DisplayProductModel displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
					_carouselModel.ProcessDisplayProductModelSelected(displayProductModel, e.Parameter);
				}
				else if (displayProductUserControl.Tag != null)
				{
					if (_carouselModel != null && _carouselModel.TitleRollupOverlayModel != null)
					{
						_carouselModel.TitleRollupOverlayModel.Visibility = Visibility.Collapsed;
					}
					_nextCarouselDirection = (CarouselDirection)displayProductUserControl.Tag;
					_forceRotation = true;
					_carouselModel.IsAnimationOn = true;
					StopTimer();
					AnimateCarouselRotation();
				}
			}
		}
		HandleWPFHit();
	}

	private void DisplayProductAddCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		HandleWPFHit();
		if (!IsRotating && IsCarouselProperlyInitialized)
		{
			DisplayProductCarouselUserControl displayProductUserControl = GetDisplayProductUserControl(e.OriginalSource);
			if (displayProductUserControl != null)
			{
				DisplayProductModel displayProductModel = displayProductUserControl.DataContext as DisplayProductModel;
				_carouselModel.ProcessAddDisplayProductModel(displayProductModel, e.Parameter);
			}
		}
	}

	private DisplayProductCarouselUserControl GetDisplayProductUserControl(object source)
	{
		FrameworkElement frameworkElement = source as FrameworkElement;
		while (frameworkElement != null && !(frameworkElement is DisplayProductCarouselUserControl))
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
		}
		return frameworkElement as DisplayProductCarouselUserControl;
	}
}
