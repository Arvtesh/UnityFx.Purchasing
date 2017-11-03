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
				else if (RadioPlatformAppStore.Checked)
				{
					var result = await UnityFx.Purchasing.Validation.ReceiptValidator.ValidateAppStoreReceiptAsync(TextReceipt.Text);
					var s = result.StatusText + Environment.NewLine + Environment.NewLine + result.RawResult;

					TextResult.Text = s;
				}
				else if (RadioPlatformGooglePlay.Checked)
				{
					// TODO
				}
			}
			catch (Exception ex)
			{
				TextResult.Text = "*** Exception: " + ex.ToString();
			}
		}

		private void ButtonTestReceipt_Click(object sender, EventArgs e)
		{
			TextReceipt.Text = UnityFx.Purchasing.Validation.ReceiptValidator.TestAppStoreReceipt;
		}
	}
}
