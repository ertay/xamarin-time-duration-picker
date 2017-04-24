using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using XamTimeDurationPicker.Droid.Dialogs;
using XamTimeDurationPicker.Droid.Views;

namespace XamTimeDurationPicker.Droid.Sample
{
    [Activity(Theme = "@style/MainTheme", Label = "XamTimeDurationPicker.Droid.Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);
            var button = FindViewById<Button>(Resource.Id.showPickerButton);
            
            button.Click += (sender, args) => new TestDurationiPickerDialog().Show(SupportFragmentManager, "testdialog");
            
        }
    }

    public class TestDurationiPickerDialog : TimeDurationPickerDialogFragment
    {
        private long testDuration;

        public override void OnDurationSet(TimeDurationPicker view, long duration)
        {
            testDuration = duration;

        }
    }
}

