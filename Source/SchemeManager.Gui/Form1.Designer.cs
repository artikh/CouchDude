namespace CouchDude.Gui
{
	partial class Form1
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
			this.DatabaseUrl = new System.Windows.Forms.TextBox();
			this.CheckButton = new System.Windows.Forms.Button();
			this.BaseDirectory = new System.Windows.Forms.TextBox();
			this.BrowseButton = new System.Windows.Forms.Button();
			this.OutputBox = new System.Windows.Forms.TextBox();
			this.GenerateButton = new System.Windows.Forms.Button();
			this.PushButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// DatabaseUrl
			// 
			this.DatabaseUrl.Location = new System.Drawing.Point(101, 9);
			this.DatabaseUrl.Name = "DatabaseUrl";
			this.DatabaseUrl.Size = new System.Drawing.Size(296, 20);
			this.DatabaseUrl.TabIndex = 0;
			// 
			// CheckButton
			// 
			this.CheckButton.Location = new System.Drawing.Point(15, 124);
			this.CheckButton.Name = "CheckButton";
			this.CheckButton.Size = new System.Drawing.Size(108, 23);
			this.CheckButton.TabIndex = 2;
			this.CheckButton.Text = "Check";
			this.CheckButton.UseVisualStyleBackColor = true;
			this.CheckButton.Click += new System.EventHandler(this.CheckButton_Click);
			// 
			// BaseDirectory
			// 
			this.BaseDirectory.Location = new System.Drawing.Point(101, 37);
			this.BaseDirectory.Name = "BaseDirectory";
			this.BaseDirectory.Size = new System.Drawing.Size(296, 20);
			this.BaseDirectory.TabIndex = 3;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = new System.Drawing.Point(403, 35);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = new System.Drawing.Size(75, 23);
			this.BrowseButton.TabIndex = 4;
			this.BrowseButton.Text = "Browse";
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
			// 
			// OutputBox
			// 
			this.OutputBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OutputBox.Location = new System.Drawing.Point(129, 64);
			this.OutputBox.Multiline = true;
			this.OutputBox.Name = "OutputBox";
			this.OutputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OutputBox.Size = new System.Drawing.Size(443, 285);
			this.OutputBox.TabIndex = 5;
			// 
			// GenerateButton
			// 
			this.GenerateButton.Location = new System.Drawing.Point(15, 153);
			this.GenerateButton.Name = "GenerateButton";
			this.GenerateButton.Size = new System.Drawing.Size(108, 23);
			this.GenerateButton.TabIndex = 6;
			this.GenerateButton.Text = "Generate";
			this.GenerateButton.UseVisualStyleBackColor = true;
			this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
			// 
			// PushButton
			// 
			this.PushButton.Location = new System.Drawing.Point(15, 182);
			this.PushButton.Name = "PushButton";
			this.PushButton.Size = new System.Drawing.Size(108, 23);
			this.PushButton.TabIndex = 7;
			this.PushButton.Text = "Push";
			this.PushButton.UseVisualStyleBackColor = true;
			this.PushButton.Click += new System.EventHandler(this.PushButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "База данных";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(83, 13);
			this.label2.TabIndex = 8;
			this.label2.Text = "Путь к данным";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 361);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.PushButton);
			this.Controls.Add(this.GenerateButton);
			this.Controls.Add(this.OutputBox);
			this.Controls.Add(this.BrowseButton);
			this.Controls.Add(this.BaseDirectory);
			this.Controls.Add(this.CheckButton);
			this.Controls.Add(this.DatabaseUrl);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox DatabaseUrl;
		private System.Windows.Forms.Button CheckButton;
		private System.Windows.Forms.TextBox BaseDirectory;
		private System.Windows.Forms.Button BrowseButton;
		private System.Windows.Forms.TextBox OutputBox;
		private System.Windows.Forms.Button GenerateButton;
		private System.Windows.Forms.Button PushButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;

	}
}

