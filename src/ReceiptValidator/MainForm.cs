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
					var result = await UnityFx.Purchasing.Validation.ReceiptValidator.ValidateAppStoreReceiptAsync(TextReceipt.Text);
					TextResult.Text = result.RawResult;
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
