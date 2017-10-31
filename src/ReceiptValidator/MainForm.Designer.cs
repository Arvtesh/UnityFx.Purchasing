namespace ReceiptValidator
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TextReceipt = new System.Windows.Forms.TextBox();
			this.RadioPlatformAndroid = new System.Windows.Forms.RadioButton();
			this.RadioPlatformIos = new System.Windows.Forms.RadioButton();
			this.PlatformGroupBox = new System.Windows.Forms.GroupBox();
			this.TextResult = new System.Windows.Forms.TextBox();
			this.ButtonValidate = new System.Windows.Forms.Button();
			this.RadioPlatformIosSandbox = new System.Windows.Forms.RadioButton();
			this.PlatformGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// TextReceipt
			// 
			this.TextReceipt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextReceipt.Location = new System.Drawing.Point(137, 12);
			this.TextReceipt.Multiline = true;
			this.TextReceipt.Name = "TextReceipt";
			this.TextReceipt.Size = new System.Drawing.Size(622, 104);
			this.TextReceipt.TabIndex = 0;
			// 
			// RadioPlatformAndroid
			// 
			this.RadioPlatformAndroid.AutoSize = true;
			this.RadioPlatformAndroid.Location = new System.Drawing.Point(6, 19);
			this.RadioPlatformAndroid.Name = "RadioPlatformAndroid";
			this.RadioPlatformAndroid.Size = new System.Drawing.Size(61, 17);
			this.RadioPlatformAndroid.TabIndex = 1;
			this.RadioPlatformAndroid.Text = "Android";
			this.RadioPlatformAndroid.UseVisualStyleBackColor = true;
			// 
			// RadioPlatformIos
			// 
			this.RadioPlatformIos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RadioPlatformIos.AutoSize = true;
			this.RadioPlatformIos.Checked = true;
			this.RadioPlatformIos.Location = new System.Drawing.Point(6, 42);
			this.RadioPlatformIos.Name = "RadioPlatformIos";
			this.RadioPlatformIos.Size = new System.Drawing.Size(42, 17);
			this.RadioPlatformIos.TabIndex = 2;
			this.RadioPlatformIos.TabStop = true;
			this.RadioPlatformIos.Text = "iOS";
			this.RadioPlatformIos.UseVisualStyleBackColor = true;
			// 
			// PlatformGroupBox
			// 
			this.PlatformGroupBox.Controls.Add(this.RadioPlatformIosSandbox);
			this.PlatformGroupBox.Controls.Add(this.RadioPlatformIos);
			this.PlatformGroupBox.Controls.Add(this.RadioPlatformAndroid);
			this.PlatformGroupBox.Location = new System.Drawing.Point(12, 12);
			this.PlatformGroupBox.Name = "PlatformGroupBox";
			this.PlatformGroupBox.Size = new System.Drawing.Size(119, 104);
			this.PlatformGroupBox.TabIndex = 3;
			this.PlatformGroupBox.TabStop = false;
			this.PlatformGroupBox.Text = "Platform";
			// 
			// TextResult
			// 
			this.TextResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextResult.Location = new System.Drawing.Point(12, 122);
			this.TextResult.Multiline = true;
			this.TextResult.Name = "TextResult";
			this.TextResult.Size = new System.Drawing.Size(747, 406);
			this.TextResult.TabIndex = 4;
			// 
			// ButtonValidate
			// 
			this.ButtonValidate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonValidate.Location = new System.Drawing.Point(684, 534);
			this.ButtonValidate.Name = "ButtonValidate";
			this.ButtonValidate.Size = new System.Drawing.Size(75, 23);
			this.ButtonValidate.TabIndex = 5;
			this.ButtonValidate.Text = "Validate";
			this.ButtonValidate.UseVisualStyleBackColor = true;
			this.ButtonValidate.Click += new System.EventHandler(this.ButtonValidate_Click);
			// 
			// RadioPlatformIosSandbox
			// 
			this.RadioPlatformIosSandbox.AutoSize = true;
			this.RadioPlatformIosSandbox.Location = new System.Drawing.Point(6, 65);
			this.RadioPlatformIosSandbox.Name = "RadioPlatformIosSandbox";
			this.RadioPlatformIosSandbox.Size = new System.Drawing.Size(87, 17);
			this.RadioPlatformIosSandbox.TabIndex = 3;
			this.RadioPlatformIosSandbox.Text = "iOS Sandbox";
			this.RadioPlatformIosSandbox.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(771, 569);
			this.Controls.Add(this.ButtonValidate);
			this.Controls.Add(this.TextResult);
			this.Controls.Add(this.PlatformGroupBox);
			this.Controls.Add(this.TextReceipt);
			this.Name = "MainForm";
			this.Text = "Purchase Receipt Validator";
			this.PlatformGroupBox.ResumeLayout(false);
			this.PlatformGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox TextReceipt;
		private System.Windows.Forms.RadioButton RadioPlatformAndroid;
		private System.Windows.Forms.RadioButton RadioPlatformIos;
		private System.Windows.Forms.GroupBox PlatformGroupBox;
		private System.Windows.Forms.TextBox TextResult;
		private System.Windows.Forms.Button ButtonValidate;
		private System.Windows.Forms.RadioButton RadioPlatformIosSandbox;
	}
}

