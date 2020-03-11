using Plugin.XFInAppBilling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XFInAppBilling.Tests
{
    extern alias amazon;
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private IXFInAppBilling GetIAPbilling()
        {
            if (App.IsAmazon)
            {
                var result = amazon.Plugin.XFInAppBilling.Amazon.CrossXFInAppBilling.Current;
                return result;
            }
            else
            {
                var result = CrossXFInAppBilling.Current;
                return result;
            }
        }

        public bool IsConnected { get; set; } = false;
        public MainPage()
        {
            InitializeComponent();
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            IsConnected = await GetIAPbilling().ConnectAsync();
        }


        private void ButtonConsumable_Clicked(object sender, EventArgs e)
        {

        }

        private async void ButtonNonConsumable_Clicked(object sender, EventArgs e)
        {
            var id = "iaptest";
            try
            {
                var purchase = await GetIAPbilling().PurchaseAsync(id, ItemType.InAppPurchase, "mypayload");

                if (purchase == null)
                {
                    await DisplayAlert(string.Empty, "Did not purchase", "OK");
                }
                else
                {
                    await DisplayAlert(string.Empty, "We did it!", "OK");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
        }

        private async void ButtonSub_Clicked(object sender, EventArgs e)
        {
            var id = "renewsub";
            try
            {
                var purchase = await GetIAPbilling().PurchaseAsync(id, ItemType.Subscription, "mypayload");

                if (purchase == null)
                {
                    await DisplayAlert(string.Empty, "Did not purchase", "OK");
                }
                else
                {
                    await DisplayAlert(string.Empty, "We did it!", "OK");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
        }

        private   void ButtonRenewingSub_Clicked(object sender, EventArgs e)
        {

        }

        private async void ButtonRestore_Clicked(object sender, EventArgs e)
        {
            try
            {
                var purchases = await GetIAPbilling().GetPurchasesAsync(ItemType.Subscription);

                if (purchases == null)
                {
                    await DisplayAlert(string.Empty, "Did not purchase", "OK");
                }
                else
                {
                    await DisplayAlert(string.Empty, "We did it!", "OK");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }
        }
    }
}
