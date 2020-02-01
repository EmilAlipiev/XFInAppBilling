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
	// Learn more about making custom code visible in the Xamarin.Forms previewer
	// by visiting https://aka.ms/xamarinforms-previewer
	[DesignTimeVisible(false)]
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private void ButtonConsumable_Clicked(object sender, EventArgs e)
		{

		}

		private async void ButtonNonConsumable_Clicked(object sender, EventArgs e)
		{
			var id = "iaptest";
			try
			{
				var purchase = await CrossXFInAppBilling.Current.PurchaseAsync(id, ItemType.InAppPurchase, "mypayload");

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
				var purchase = await CrossXFInAppBilling.Current.PurchaseAsync(id, ItemType.Subscription, "mypayload");

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

		private async void ButtonRenewingSub_Clicked(object sender, EventArgs e)
		{

		}

		private async void ButtonRestore_Clicked(object sender, EventArgs e)
		{
			try
			{
				var purchases = await CrossXFInAppBilling.Current.GetPurchasesAsync(ItemType.Subscription);

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
