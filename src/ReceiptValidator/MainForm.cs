using System;
using System.Windows.Forms;

namespace ReceiptValidator
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private async void ButtonValidate_Click(object sender, EventArgs e)
		{
			try
			{
				TextResult.Text = "Please wait..";

				if (string.IsNullOrEmpty(TextReceipt.Text))
				{
					TextResult.Text = "*** Receipt text is empty";
				}
				else if (RadioPlatformIos.Checked)
				{
					TextResult.Text = await UnityFx.Purchasing.Validation.ReceiptValidator.ValidateAppStoreReceiptAsync(TextReceipt.Text, false);
				}
				else if (RadioPlatformIosSandbox.Checked)
				{
					TextResult.Text = await UnityFx.Purchasing.Validation.ReceiptValidator.ValidateAppStoreReceiptAsync(TextReceipt.Text, true);
				}
				else if (RadioPlatformAndroid.Checked)
				{
					// TODO
				}
			}
			catch (Exception ex)
			{
				TextResult.Text = "*** Exception: " + ex.ToString();
			}
		}
	}
}
