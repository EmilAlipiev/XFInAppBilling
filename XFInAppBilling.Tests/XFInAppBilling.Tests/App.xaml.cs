using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XFInAppBilling.Tests
{
    public partial class App : Application
    {
        public static bool IsAmazon { get; set; } = false;
        public App(bool isAmazon=false)
        {
            IsAmazon = isAmazon;
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
