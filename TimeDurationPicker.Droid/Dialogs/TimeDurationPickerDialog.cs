using Android.Content;
using Android.OS;
using Android.Views;
using XamTimeDurationPicker.Droid.Views;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace XamTimeDurationPicker.Droid.Dialogs
{
    public class TimeDurationPickerDialog : AlertDialog, IDialogInterfaceOnClickListener
    {
        private static string DURATION = "duration";
    private TimeDurationPicker _durationInputView;
    private OnDurationSetListener _durationSetListener;

        /// <summary>
        /// The callback used to indicate the user is done entering the duration.
        /// </summary>
        public interface OnDurationSetListener
        {
            /// <summary>
            /// Called when the user leaves the dialog using the OK button.
            /// </summary>
            /// <param name="view">The picker object.</param>
            /// <param name="duration">The duration that was set</param>
            void OnDurationSet(TimeDurationPicker view, long duration);
        }

        /// <summary>
        /// Creates a time picker dialog.
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="listener">The listener to get the set duration.</param>
        /// <param name="duration">The default duration.</param>
        public TimeDurationPickerDialog(Context context, OnDurationSetListener listener, long duration)
            : base(context)
        {
            
            _durationSetListener = listener;

            LayoutInflater inflater = LayoutInflater.From(context);
            View view = inflater.Inflate(Resource.Layout.time_duration_picker_dialog, null);
            SetView(view);
            SetButton(-1, context.GetString(Resource.String.ok), this);
            SetButton(-2, context.GetString(Resource.String.cancel), this);

            _durationInputView = (TimeDurationPicker)view;
            _durationInputView.Duration = duration;
        }

        /// <summary>
        /// Creates a time picker dialog.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="listener">The listener to get the set duration.</param>
        /// <param name="duration">The default duration in milliseconds.</param>
        /// <param name="timeUnits">The default timeunits.</param>
        public TimeDurationPickerDialog(Context context, OnDurationSetListener listener, long duration, int timeUnits)
            : this(context, listener, duration)
        {
            _durationInputView.TimeUnits = timeUnits;
        }

        /// <summary>
        /// Gets or sets  the duration in milliseconds. 
        /// </summary>
        public long Duration
        {
            get { return _durationInputView.Duration; }
            set { _durationInputView.Duration = value; }
        }

        /// <summary>
        /// Fires when a positive or negative button is clicked.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="which"></param>
        public void OnClick(IDialogInterface dialog, int which)
        {
            switch (which)
            {
                case -1:
                    // ok button clicked
                    _durationSetListener?.OnDurationSet(_durationInputView, _durationInputView.Duration);
                    break;
                case -2:
                    // cancel button clicked
                    Cancel();
                    break;

            }
        }

        public override Bundle OnSaveInstanceState()
        {
            Bundle state = base.OnSaveInstanceState();
            state.PutLong(DURATION, _durationInputView.Duration);
            return state;
        }

        public override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            long duration = savedInstanceState.GetLong(DURATION);
            _durationInputView.Duration = duration;
        }
    }
}