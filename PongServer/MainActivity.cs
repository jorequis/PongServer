using Android.App;
using Android.Widget;
using Android.OS;

using System.Linq;
using System.Threading;
using Android.Text.Method;

namespace PongServer
{
    [Activity(Label = "PongServer", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private static TextView logTextView;
        private static ScrollView scrollView;
        public delegate void LogDelegate(string log);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);

            scrollView = FindViewById<ScrollView>(Resource.Id.scrollView1);
            logTextView = FindViewById<TextView>(Resource.Id.textView1);
            logTextView.Text = "";

            //new Thread(Loop).Start();
            new Server((s) => { Log(s); });
        }

        public void Loop()
        {
            while (true)
            {
                Thread.Sleep(500);
            }
        }

        private void Log(string log)
        {
            RunOnUiThread(() =>
            {
                logTextView.Append(log + "\n");
                scrollView.Post(() => { scrollView.FullScroll(Android.Views.FocusSearchDirection.Down); });
            });
        }
    }
}

