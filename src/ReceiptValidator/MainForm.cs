using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net;
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
					TextResult.Text = await ValidateIos(TextReceipt.Text, false);
				}
				else if (RadioPlatformIosSandbox.Checked)
				{
					TextResult.Text = await ValidateIos(TextReceipt.Text, true);
				}
				else if (RadioPlatformAndroid.Checked)
				{
					TextResult.Text = await ValidateAndroid(TextReceipt.Text);
				}
			}
			catch (Exception ex)
			{
				TextResult.Text = "*** Exception: " + ex.ToString();
			}
		}

		private async Task<string> ValidateIos(string receipt, bool sandbox)
		{
			var storeUrl = sandbox ? "https://sandbox.itunes.apple.com/verifyReceipt" : "https://buy.itunes.apple.com/verifyReceipt";
			var json = string.Format("{{\"receipt-data\":\"{0}\"}}", receipt);
			var ascii = new ASCIIEncoding();
			var postBytes = Encoding.UTF8.GetBytes(json);

			var request = WebRequest.Create(storeUrl);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = postBytes.Length;

			using (var stream = await request.GetRequestStreamAsync())
			{
				await stream.WriteAsync(postBytes, 0, postBytes.Length);
				await stream.FlushAsync();
			}

			var sendResponse = await request.GetResponseAsync();

			using (var streamReader = new StreamReader(sendResponse.GetResponseStream()))
			{
				return await streamReader.ReadToEndAsync();
			}
		}

		private async Task<string> ValidateAndroid(string receipt)
		{
			throw new NotImplementedException();
		}
	}
}
