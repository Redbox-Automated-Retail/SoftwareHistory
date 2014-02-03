using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace VisualHint.SmartPropertyGrid;

[ToolboxItem(false)]
public abstract class GenericTrackBar : TrackBar, ISupportInitialize
{
	protected object mMinimum;

	protected object mMaximum;

	protected object mSmallChange;

	protected object mLargeChange;

	protected object mResolution;

	protected object mValue;

	protected object mTickFrequency;

	private int _initializing;

	internal Type _baseType;

	[TypeConverter(typeof(TrackBarPropertyConverter))]
	[Category("Behavior")]
	public new virtual object Minimum
	{
		get
		{
			return mMinimum;
		}
		set
		{
			mMinimum = value;
			SetupTrackBar();
		}
	}

	[TypeConverter(typeof(TrackBarPropertyConverter))]
	[Category("Behavior")]
	public new virtual object Maximum
	{
		get
		{
			return mMaximum;
		}
		set
		{
			mMaximum = value;
			SetupTrackBar();
		}
	}

	[TypeConverter(typeof(TrackBarPropertyConverter))]
	[Category("Behavior")]
	public virtual object Resolution
	{
		get
		{
			return mResolution;
		}
		set
		{
			mResolution = value;
			SetupTrackBar();
		}
	}

	[TypeConverter(typeof(TrackBarPropertyConverter))]
	[Category("Appearance")]
	public new virtual object SmallChange
	{
		get
		{
			return mSmallChange;
		}
		set
		{
			mSmallChange = value;
			SetupTrackBar();
		}
	}

	[TypeConverter(typeof(TrackBarPropertyConverter))]
	[Category("Appearance")]
	public new virtual object LargeChange
	{
		get
		{
			return mLargeChange;
		}
		set
		{
			mLargeChange = value;
			SetupTrackBar();
		}
	}

	[TypeConverter(typeof(TrackBarPropertyConverter))]
	[Category("Appearance")]
	public new virtual object TickFrequency
	{
		get
		{
			return mTickFrequency;
		}
		set
		{
			mTickFrequency = value;
			SetupTrackBar();
		}
	}

	[Bindable(true)]
	[Category("Behavior")]
	[TypeConverter(typeof(TrackBarPropertyConverter))]
	public new virtual object Value
	{
		get
		{
			return mValue;
		}
		set
		{
			mValue = value;
			SetupTrackBar();
		}
	}

	public GenericTrackBar(Type baseType)
	{
		_baseType = baseType;
	}

	public void ChangeType(Type baseType)
	{
		_baseType = baseType;
		ResetSettings();
	}

	public new void BeginInit()
	{
		base.BeginInit();
		_initializing++;
	}

	public new void EndInit()
	{
		_initializing--;
		if (_initializing == 0)
		{
			base.EndInit();
			SetupTrackBar();
		}
	}

	protected void SetupTrackBar()
	{
		if (_initializing == 0)
		{
			int num = base.Value;
			base.Minimum = 0;
			base.Maximum = GetIntegerSteps();
			if (num != base.Value)
			{
				mValue = ConvertIntegerToBaseType(base.Value);
			}
			base.LargeChange = ConvertStepTypeToInteger(mLargeChange);
			base.SmallChange = ConvertStepTypeToInteger(mSmallChange);
			base.TickFrequency = ConvertStepTypeToInteger(mTickFrequency);
			base.Value = ConvertBaseTypeToInteger(mValue);
		}
	}

	protected override void OnValueChanged(EventArgs e)
	{
		mValue = ConvertIntegerToBaseType(base.Value);
		base.OnValueChanged(e);
	}

	public abstract void ResetSettings();

	protected abstract int GetIntegerSteps();

	protected abstract int ConvertStepTypeToInteger(object value);

	protected abstract int ConvertBaseTypeToInteger(object value);

	protected abstract object ConvertIntegerToBaseType(int position);

	public abstract object GetValueMinusSmallChange();

	public abstract object GetValuePlusSmallChange();

	public abstract object GetValueMinusLargeChange();

	public abstract object GetValuePlusLargeChange();

	public abstract int GetMaxWidth(Graphics graphics, Font font);
}
