using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using XamTimeDurationPicker.Droid.Helpers;
using Math = System.Math;
using StringBuilder = Java.Lang.StringBuilder;

namespace XamTimeDurationPicker.Droid.Views
{
    [Register("XamTimeDurationPicker.Droid.Views.TimeDurationPicker")]
    public class TimeDurationPicker : FrameLayout
    {
        public const int HH_MM_SS = 0;
        public const int HH_MM = 1;
        public const int MM_SS = 2;

        private int _timeUnits = HH_MM_SS;

        private TimeDurationString _input = new TimeDurationString();
        private View _displayRow;
    private View _durationView;
    private TextView _hoursView;
    private TextView _minutesView;
    private TextView _secondsView;
    private TextView[] _displayViews;
    private TextView[] _unitLabelViews;
    private ImageButton _backspaceButton;
    private ImageButton _clearButton;
    private View _separatorView;
    private View _numPad;
    private Button[] _numPadButtons;
    private Button _numPadMeasureButton;

    private OnDurationChangedListener _changeListener = null;
        private TextView _secondsLabel;
        private TextView _hoursLabel;
        private TextView _minutesLabel;

        #region change listener interface

        /// <summary>
        /// Duration changed listener. Set this using SetDurationChangedListener
        /// to receive duration changed events.
        /// </summary>
        public interface OnDurationChangedListener
        {
            /// <summary>
            /// Called whenever the input (the displayed duration string) changes.
            /// </summary>
            /// <param name="view">View that fired the event</param>
            /// <param name="duration">The new duration in milli seconds</param>
            void OnDurationChanged(TimeDurationPicker view, long duration);
        }

        #endregion

        #region constructors

        public TimeDurationPicker(Context context) : this(context, null) {}

        public TimeDurationPicker(Context context, IAttributeSet attrs)
            : this(context, attrs, Resource.Attribute.timeDurationPickerStyle)
        {
        }

        public TimeDurationPicker(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            Inflate(context, Resource.Layout.time_duration_picker, this);

            // find views
            _displayRow = FindViewById(Resource.Id.displayRow);
            _durationView = FindViewById(Resource.Id.duration);
            _hoursView = FindViewById<TextView>(Resource.Id.hours);
            _minutesView = FindViewById<TextView>(Resource.Id.minutes);
            _secondsView = FindViewById<TextView>(Resource.Id.seconds);
            _displayViews = new TextView[] {_hoursView, _minutesView, _secondsView};

            _hoursLabel = FindViewById<TextView>(Resource.Id.hoursLabel);
            _minutesLabel = FindViewById<TextView>(Resource.Id.minutesLabel);
            _secondsLabel = FindViewById<TextView>(Resource.Id.secondsLabel);
            _unitLabelViews = new TextView[] {_hoursLabel, _minutesLabel, _secondsLabel};

            _backspaceButton = FindViewById<ImageButton>(Resource.Id.backspace);
            _clearButton = FindViewById<ImageButton>(Resource.Id.clear);

            _separatorView = FindViewById(Resource.Id.separator);

            _numPad = FindViewById(Resource.Id.numPad);
            _numPadMeasureButton = FindViewById<Button>(Resource.Id.numPadMeasure);
            _numPadButtons = new Button[]
            {
                FindViewById<Button>(Resource.Id.numPad1), FindViewById<Button>(Resource.Id.numPad2),
                FindViewById<Button>(Resource.Id.numPad3),
                FindViewById<Button>(Resource.Id.numPad4), FindViewById<Button>(Resource.Id.numPad5),
                FindViewById<Button>(Resource.Id.numPad6),
                FindViewById<Button>(Resource.Id.numPad7), FindViewById<Button>(Resource.Id.numPad8),
                FindViewById<Button>(Resource.Id.numPad9),
                FindViewById<Button>(Resource.Id.numPad0), FindViewById<Button>(Resource.Id.numPad00)
            };

            // apply style
            TypedArray attributes = context.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.TimeDurationPicker,
                defStyleAttr, 0);
            try
            {
                ApplyPadding(attributes, Resource.Styleable.TimeDurationPicker_numPadButtonPadding, _numPadButtons);

                ApplyTextAppearance(context, attributes, Resource.Styleable.TimeDurationPicker_textAppearanceDisplay,
                    _displayViews);
                ApplyTextAppearance(context, attributes, Resource.Styleable.TimeDurationPicker_textAppearanceButton,
                    _numPadButtons);
                ApplyTextAppearance(context, attributes, Resource.Styleable.TimeDurationPicker_textAppearanceUnit,
                    _unitLabelViews);

                ApplyIcon(attributes, Resource.Styleable.TimeDurationPicker_backspaceIcon, _backspaceButton);
                ApplyIcon(attributes, Resource.Styleable.TimeDurationPicker_clearIcon, _clearButton);

                ApplyBackgroundColor(attributes, Resource.Styleable.TimeDurationPicker_separatorColor, _separatorView);
                ApplyBackgroundColor(attributes, Resource.Styleable.TimeDurationPicker_durationDisplayBackground,
                    _displayRow);

                ApplyUnits(attributes, Resource.Styleable.TimeDurationPicker_timeUnits);
            }
            finally
            {
                attributes.Recycle();
            }

            // init actions
            UpdateUnits();

            _backspaceButton.Click += (sender, args) => OnBackspace();
            _clearButton.Click += (IntentSender, args) => OnClear();

            foreach (var button in _numPadButtons)
            {
                button.Click += (sender, args) => OnNumberClick(button.Text);
            }

            // init default value
            UpdateHoursMinutesSeconds();
        }

        #endregion

        private void UpdateUnits()
        {
            _hoursView.Visibility = _timeUnits == HH_MM_SS || _timeUnits == HH_MM ? ViewStates.Visible : ViewStates.Gone;
            _hoursLabel.Visibility = _timeUnits == HH_MM_SS || _timeUnits == HH_MM ? ViewStates.Visible : ViewStates.Gone;
            _secondsView.Visibility = _timeUnits == HH_MM_SS || _timeUnits == MM_SS ? ViewStates.Visible : ViewStates.Gone;
            _secondsLabel.Visibility = _timeUnits == HH_MM_SS || _timeUnits == MM_SS ? ViewStates.Visible : ViewStates.Gone;

            _input.UpdateTimeUnits(_timeUnits);
        }

        private void ApplyUnits(TypedArray attrs, int attributeIndex)
        {
            if (attrs.HasValue(attributeIndex))
            {
                _timeUnits = attrs.GetInt(attributeIndex, 0);
            }
        }

        #region properties

        /// <summary>
        /// Gets or sets the duration in milliseconds.
        /// </summary>
        public long Duration
        {
            get { return _input.GetDuration(); }
            set
            {
                _input.SetDuration(value);
                UpdateHoursMinutesSeconds();
            }
        }

        /// <summary>
        /// Sets the time units.
        /// 0 = HH_MM_SS
        /// 1 = HH_MM
        /// 2 = MM_SS
        /// TODO: Consider switching this to Enum 
        /// </summary>
        public int TimeUnits
        {
            set
            {
                _timeUnits = value;
                UpdateUnits();
            }
        }

        #endregion

        /// <summary>
        /// Sets the duration chaanged listener to be informed for entered duration changes.
        /// </summary>
        /// <param name="listener"></param>
        public void SetOnDurationChangeListener(OnDurationChangedListener listener)
        {
            _changeListener = listener;
        }

        /// <summary>
        /// Sets the text appearance for the entered duration (the large numbers in the upper area).
        /// </summary>
        /// <param name="resId">Resource id of the style describing the text appearance.</param>
        public void SetDisplayTextAppearance(int resId)
        {
            ApplyTextAppearance(Context, resId, _displayViews);
        }

        /// <summary>
        /// Sets the text appearance for the small unit labels ("h", "m", "s") in the upper display area.
        /// </summary>
        /// <param name="resId">Resource id of the style describing the text appearance.</param>
        public void SetUnitTextAppearance(int resId)
        {
            ApplyTextAppearance(Context, resId, _unitLabelViews);
        }

        /// <summary>
        /// Sets the text appearance for the number pad buttons.
        /// </summary>
        /// <param name="resId">Resource id of the style describing the text appearance.</param>
        public void SetButtonTextAppearance(int resId)
        {
            ApplyTextAppearance(Context, resId, _numPadButtons);
        }

        /// <summary>
        /// Sets a drawable for the backspace button.
        /// </summary>
        /// <param name="icon"></param>
        public void SetBackspaceIcon(Drawable icon)
        {
            _backspaceButton.SetImageDrawable(icon);
        }

        /// <summary>
        /// Sets a drawabale for the clear button.
        /// </summary>
        /// <param name="icon"></param>
        public void SetClearIcon(Drawable icon)
        {
            _clearButton.SetImageDrawable(icon);
        }

        /// <summary>
        /// Sets the color of the separator line between the duration display and the number pad.
        /// </summary>
        /// <param name="color"></param>
        public void SetSeparatorColor(int color)
        {
            _separatorView.SetBackgroundColor(new Color(color));
        }

        /// <summary>
        /// Sets the background color of the upper duration display area.
        /// </summary>
        /// <param name="color"></param>
        public void SetDurationDisplayBackgroundColor(int color)
        {
            _displayRow.SetBackgroundColor(new Color(color));
        }

        /// <summary>
        /// Sets the padding to be used for the number pad buttons.
        /// </summary>
        /// <param name="padding">Padding in pixels</param>
        public void SetNumPadButtonPadding(int padding)
        {
            ApplyPadding(padding, _numPadButtons);
        }

        #region style helpers

        private void ApplyPadding(TypedArray attrs, int attributeIndex, View[] targetViews)
        {
            int padding = attrs.GetDimensionPixelSize(attributeIndex, -1);
            if (padding > -1)
            {
                ApplyPadding(padding, targetViews);
            }
        }

        private void ApplyPadding(int padding, View[] targetViews)
        {
            foreach (var view in targetViews)
            {
                view.SetPadding(padding,padding,padding,padding);
            }
        }

        private void ApplyTextAppearance(Context context, TypedArray attrs, int attributeIndex, TextView[] targetViews)
        {
            int id = attrs.GetResourceId(attributeIndex, 0);
            if (id != 0)
            {
                ApplyTextAppearance(context, id, targetViews);
            }
        }

        private void ApplyTextAppearance(Context context, int resId, TextView[] targetViews)
        {
            foreach (var view in targetViews)
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                {
                    view.SetTextAppearance(context, resId);
                }
                else
                {
                    view.SetTextAppearance(resId);
                }
            }
        }

        private void ApplyIcon(TypedArray attrs, int attributeIndex, ImageView targetView)
        {
            Drawable icon = attrs.GetDrawable(attributeIndex);
            if (icon != null)
            {
                targetView.SetImageDrawable(icon);
            }
        }

        private void ApplyBackgroundColor(TypedArray attrs, int attributeIndex, View targetView)
        {
            if (attrs.HasValue(attributeIndex))
            {
                int color = attrs.GetColor(attributeIndex, 0);
                targetView.SetBackgroundColor(new Color(color));
            }
        }

        private void ApplyLeftMargin(int margin, View[] targetViews)
        {
            foreach(View view in targetViews)
            {
                LinearLayout.LayoutParams layoutParams = (LinearLayout.LayoutParams)view.LayoutParameters;
            layoutParams.SetMargins(margin, layoutParams.TopMargin, layoutParams.RightMargin, layoutParams.BottomMargin);
                view.LayoutParameters = layoutParams;
            }
        }

        #endregion

        #region event handlers

        private void OnBackspace()
        {
            _input.PopDigit();
            UpdateHoursMinutesSeconds();
        }

        private void OnClear()
        {
            _input.Clear();
            UpdateHoursMinutesSeconds();
        }

        private void OnNumberClick(string digits)
        {
            _input.PushNumber(digits);
            UpdateHoursMinutesSeconds();
        }

        private void UpdateHoursMinutesSeconds()
        {
            _hoursView.Text = _input.GetHoursString();
            _minutesView.Text = _input.GetMinutesString();
            _secondsView.Text = _input.GetSecondsString();
            FireDurationChangeListener();
        }

        private void FireDurationChangeListener()
        {
            if (_changeListener != null)
            {
                _changeListener.OnDurationChanged(this, _input.GetDuration());
            }
        }

        #endregion

        #region layouting

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int touchableSize = Resources.GetDimensionPixelSize(Resource.Dimension.touchable);
            int dummyMeasureSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);

            // set spacing between units
            _hoursView.Measure(dummyMeasureSpec, dummyMeasureSpec);
            TextView unitLabelView = _unitLabelViews[2];
            unitLabelView.Measure(dummyMeasureSpec, dummyMeasureSpec);
            int unitSpacing = Math.Max(_hoursView.MeasuredWidth/3, (int) (1.2f*unitLabelView.MeasuredWidth));
            ApplyLeftMargin(unitSpacing, new View[] {_minutesView, _secondsView});

            // calculate size for display row
            _durationView.Measure(dummyMeasureSpec, dummyMeasureSpec);
            int minDisplayWidth = _durationView.MeasuredWidth + 2*touchableSize;
            int minDisplayHeight = Math.Max(_durationView.MeasuredHeight, touchableSize);

            // calculate size for num pad
            _numPadMeasureButton.Measure(dummyMeasureSpec, dummyMeasureSpec);
            int minNumPadButtonSize =
                Math.Max(Math.Max(_numPadMeasureButton.MeasuredHeight, _numPadMeasureButton.MeasuredWidth),
                    touchableSize);
            int minNumPadWidth = 3*minNumPadButtonSize;
            int minNumPadHeight = 4*minNumPadButtonSize;

            // calculate overall size
            int minWidth = Math.Max(minDisplayWidth, minNumPadWidth);
            int minHeight = minDisplayHeight + minNumPadHeight;

            // respect measure spec
            int availableWidth = MeasureSpec.GetSize(widthMeasureSpec);
            int availableHeight = MeasureSpec.GetSize(heightMeasureSpec);
            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);

            int preferredWidth = widthMode == MeasureSpecMode.Exactly ? availableWidth : minWidth;
            int preferredHeight = heightMode == MeasureSpecMode.Exactly ? availableHeight : minHeight;

            // measure the display
            int displayRowWidth = Math.Max(minDisplayWidth, preferredWidth);
            int displayRowHeight = minDisplayHeight;
            _displayRow.Measure(MeasureSpec.MakeMeasureSpec(displayRowWidth, MeasureSpecMode.Exactly),
                MeasureSpec.MakeMeasureSpec(displayRowHeight, MeasureSpecMode.Exactly));

            // measure the numPad
            // if we have more space available, we can try to grow the num pad
            int numPadWidth = Math.Max(minNumPadHeight, displayRowWidth);
            int numPadHeight = Math.Max(minNumPadHeight, preferredHeight - displayRowHeight);
            _numPad.Measure(MeasureSpec.MakeMeasureSpec(numPadWidth, MeasureSpecMode.Exactly),
                MeasureSpec.MakeMeasureSpec(numPadHeight, MeasureSpecMode.Exactly));

            // forward calculated size to super implementation
            int width = Math.Max(displayRowWidth, numPadWidth);
            int height = displayRowHeight + numPadHeight;
            SetMeasuredDimension(width, height);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            int width = right - left;

            // layout display row
            int displayRowWidth = _displayRow.MeasuredWidth;
            int displayRowHeight = _displayRow.MeasuredHeight;
            int displayRowX = (width - displayRowWidth) / 2;
            _displayRow.Layout(displayRowX, 0, displayRowX + displayRowWidth, displayRowHeight);

            // layout num pad
            int numPadWidth = _numPad.MeasuredWidth;
            int numPadHeight = _numPad.MeasuredHeight;
            int numPadX = (width - numPadWidth) / 2;
            int numPadY = displayRowHeight;
            _numPad.Layout(numPadX, numPadY, numPadX + numPadWidth, numPadY + numPadHeight);
        }

        #endregion

        #region state handling

        protected override IParcelable OnSaveInstanceState()
        {
                    return new SavedState(base.OnSaveInstanceState(), _input.GetInputString());

        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            if (!(state is SavedState))
            {
                throw new IllegalArgumentException("Expected state of class " + typeof(SavedState).Name +
                                                   " but received state of class " + state.GetType().Name);
            }
            SavedState savedStated = (SavedState) state;
            base.OnRestoreInstanceState(savedStated.SuperState);
            _input.Clear();
            _input.PushNumber(savedStated.DurationInput);
            UpdateHoursMinutesSeconds();
        }

        #endregion

        /// <summary>
        /// Encapsulates the digit _input logic and text to _duration conversion logic.
        /// </summary>
        private class TimeDurationString
        {
            private int _timeUnits;
            private int _maxDigits = 6;
            private long _duration = 0;

            private StringBuilder _input;

            public TimeDurationString()
            {
                _input = new StringBuilder(_maxDigits);
                PadWithZeros();
            }

            public void UpdateTimeUnits(int timeUnits)
            {
                this._timeUnits = timeUnits;
                SetMaxDigits(timeUnits);
            }

            private void SetMaxDigits(int timeUnits)
            {
                if (timeUnits == TimeDurationPicker.HH_MM_SS)
                    _maxDigits = 6;
                else
                    _maxDigits = 4;
                SetDuration(_duration);
            }

            public void PushNumber(string digits)
            {
                for (int i = 0; i < digits.Length; ++i)
                {
                    PushDigit(digits[i]);
                }
            }

            public void PushDigit(char digit)
            {
                if (!char.IsDigit(digit))
                    throw new IllegalArgumentException("Only numbers are allowed");

                RemoveLeadingZeros();
                if (_input.Length() < _maxDigits && (_input.Length() > 0 || digit != '0'))
                {
                    _input.Append(digit);
                }
                PadWithZeros();
            }

            public void PopDigit()
            {
                if (_input.Length() > 0)
                    _input.DeleteCharAt(_input.Length() - 1);
                PadWithZeros();
            }

            public void Clear()
            {
                _input.SetLength(0);
                PadWithZeros();
            }

            public string GetHoursString()
            {
                return _timeUnits == HH_MM_SS || _timeUnits == HH_MM ? _input.Substring(0, 2) : "00";
            }

            public string GetMinutesString()
            {
                if (_timeUnits == HH_MM_SS || _timeUnits == HH_MM) return _input.Substring(2, 4);
                else if (_timeUnits == MM_SS) return _input.Substring(0, 2);
                else return "00";
            }

            public string GetSecondsString()
            {
                if (_timeUnits == HH_MM_SS) return _input.Substring(4, 6);
                else if (_timeUnits == MM_SS) return _input.Substring(2, 4);
                else return "00";
            }

            public string GetInputString()
            {
                return _input.ToString();
            }

            public long GetDuration()
            {
                int hours = int.Parse(GetHoursString());
                int minutes = int.Parse(GetMinutesString());
                int seconds = int.Parse(GetSecondsString());
                return TimeDurationUtil.DurationOf(hours, minutes, seconds);
            }

            public void SetDuration(long millis)
            {
                _duration = millis;
                SetDuration(
                    TimeDurationUtil.HoursOf(millis),
                    _timeUnits == MM_SS ? TimeDurationUtil.MinutesOf(millis) : TimeDurationUtil.MinutesInHourOf(millis),
                    TimeDurationUtil.SecondsInMinuteOf(millis));
            }

            private void SetDuration(long hours, long minutes, long seconds)
            {
                if (hours > 99 || minutes > 99)
                    SetDurationString("99", "99", "99");
                else
                    SetDurationString(StringFragment(hours), StringFragment(minutes), StringFragment(seconds));
            }

            private void SetDurationString(string hours, string minutes, string seconds)
            {
                _input.SetLength(0);
                if (_timeUnits == HH_MM || _timeUnits == HH_MM_SS)
                    _input.Append(hours);
                _input.Append(minutes);
                if (_timeUnits == HH_MM_SS || _timeUnits == MM_SS)
                    _input.Append(seconds);
            }

            private void RemoveLeadingZeros()
            {
                while (_input.Length() > 0 && _input.CharAt(0) == '0')
                    _input.DeleteCharAt(0);
            }

            private void PadWithZeros()
            {
                while (_input.Length() < _maxDigits)
                    _input.Insert(0, '0');
            }

            private string StringFragment(long value)
            {
                return (value < 10 ? "0" : "") + value;
            }
        }

        public class SavedState : BaseSavedState
        {
            public string DurationInput { get; set; }

            public SavedState(IParcelable baseState, string durationInput)
                : base(baseState)
            {
                DurationInput = durationInput;
            }

            public SavedState(Parcel source) : base(source)
            {
                DurationInput = source.ReadString();
            }

            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);
                dest.WriteString(DurationInput);
            }


        }
    }
}