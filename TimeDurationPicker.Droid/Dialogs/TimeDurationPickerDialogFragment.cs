using Android.App;
using Android.OS;
using XamTimeDurationPicker.Droid.Views;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace XamTimeDurationPicker.Droid.Dialogs
{
    public abstract class TimeDurationPickerDialogFragment : DialogFragment, TimeDurationPickerDialog.OnDurationSetListener
    {
        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            return new TimeDurationPickerDialog(Activity, this, GetInitialDuration(), SetTimeUnits());
        }

        /// <summary>
        /// The duration to be shown as default value when the dialog appears.
        /// </summary>
        /// <returns></returns>
        protected virtual long GetInitialDuration()
        {
            return 0;
        }

        protected virtual int SetTimeUnits()
        {
            return TimeDurationPicker.HH_MM_SS;
        }

        public abstract void OnDurationSet(TimeDurationPicker view, long duration);
    }
}