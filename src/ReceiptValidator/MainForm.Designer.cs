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
			this.RadioPlatformGooglePlay = new System.Windows.Forms.RadioButton();
			this.RadioPlatformAppStore = new System.Windows.Forms.RadioButton();
			this.PlatformGroupBox = new System.Windows.Forms.GroupBox();
			this.TextResult = new System.Windows.Forms.TextBox();
			this.ButtonValidate = new System.Windows.Forms.Button();
			this.ButtonTestReceipt = new System.Windows.Forms.Button();
			this.RadioPlatformAmazon = new System.Windows.Forms.RadioButton();
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
			// RadioPlatformGooglePlay
			// 
			this.RadioPlatformGooglePlay.AutoSize = true;
			this.RadioPlatformGooglePlay.Location = new System.Drawing.Point(6, 19);
			this.RadioPlatformGooglePlay.Name = "RadioPlatformGooglePlay";
			this.RadioPlatformGooglePlay.Size = new System.Drawing.Size(79, 17);
			this.RadioPlatformGooglePlay.TabIndex = 1;
			this.RadioPlatformGooglePlay.Text = "GooglePlay";
			this.RadioPlatformGooglePlay.UseVisualStyleBackColor = true;
			// 
			// RadioPlatformAppStore
			// 
			this.RadioPlatformAppStore.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RadioPlatformAppStore.AutoSize = true;
			this.RadioPlatformAppStore.Checked = true;
			this.RadioPlatformAppStore.Location = new System.Drawing.Point(6, 42);
			this.RadioPlatformAppStore.Name = "RadioPlatformAppStore";
			this.RadioPlatformAppStore.Size = new System.Drawing.Size(69, 17);
			this.RadioPlatformAppStore.TabIndex = 2;
			this.RadioPlatformAppStore.TabStop = true;
			this.RadioPlatformAppStore.Text = "AppStore";
			this.RadioPlatformAppStore.UseVisualStyleBackColor = true;
			// 
			// PlatformGroupBox
			// 
			this.PlatformGroupBox.Controls.Add(this.RadioPlatformAmazon);
			this.PlatformGroupBox.Controls.Add(this.RadioPlatformAppStore);
			this.PlatformGroupBox.Controls.Add(this.RadioPlatformGooglePlay);
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
			// ButtonTestReceipt
			// 
			this.ButtonTestReceipt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ButtonTestReceipt.Location = new System.Drawing.Point(12, 534);
			this.ButtonTestReceipt.Name = "ButtonTestReceipt";
			this.ButtonTestReceipt.Size = new System.Drawing.Size(145, 23);
			this.ButtonTestReceipt.TabIndex = 8;
			this.ButtonTestReceipt.Text = "Test AppStore Receipt";
			this.ButtonTestReceipt.UseVisualStyleBackColor = true;
			this.ButtonTestReceipt.Click += new System.EventHandler(this.ButtonTestReceipt_Click);
			// 
			// RadioAmazon
			// 
			this.RadioPlatformAmazon.AutoSize = true;
			this.RadioPlatformAmazon.Location = new System.Drawing.Point(6, 65);
			this.RadioPlatformAmazon.Name = "RadioAmazon";
			this.RadioPlatformAmazon.Size = new System.Drawing.Size(88, 17);
			this.RadioPlatformAmazon.TabIndex = 3;
			this.RadioPlatformAmazon.Text = "AmazonStore";
			this.RadioPlatformAmazon.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(771, 569);
			this.Controls.Add(this.ButtonTestReceipt);
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
		private System.Windows.Forms.RadioButton RadioPlatformGooglePlay;
		private System.Windows.Forms.RadioButton RadioPlatformAppStore;
		private System.Windows.Forms.GroupBox PlatformGroupBox;
		private System.Windows.Forms.TextBox TextResult;
		private System.Windows.Forms.Button ButtonValidate;
		private System.Windows.Forms.Button ButtonTestReceipt;
		private System.Windows.Forms.RadioButton RadioPlatformAmazon;
	}
}

