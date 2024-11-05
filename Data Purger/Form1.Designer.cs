namespace Data_Purger
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            comboBoxDrive = new ComboBox();
            btnWipeDrive = new Button();
            trackBarBufferSize = new TrackBar();
            labelBufferSize = new Label();
            progressBar = new ProgressBar();
            logTextBox = new TextBox();
            numericPasses = new NumericUpDown();
            checkBoxQuick = new CheckBox();
            btnCancel = new Button();
            lblPasses = new Label();
            lblDrive = new Label();
            ((System.ComponentModel.ISupportInitialize)trackBarBufferSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericPasses).BeginInit();
            SuspendLayout();
            // 
            // comboBoxDrive
            // 
            comboBoxDrive.FormattingEnabled = true;
            comboBoxDrive.Location = new Point(79, 48);
            comboBoxDrive.Name = "comboBoxDrive";
            comboBoxDrive.Size = new Size(121, 23);
            comboBoxDrive.TabIndex = 0;
            // 
            // btnWipeDrive
            // 
            btnWipeDrive.Location = new Point(82, 102);
            btnWipeDrive.Name = "btnWipeDrive";
            btnWipeDrive.RightToLeft = RightToLeft.No;
            btnWipeDrive.Size = new Size(75, 23);
            btnWipeDrive.TabIndex = 1;
            btnWipeDrive.Text = "Start";
            btnWipeDrive.UseVisualStyleBackColor = true;
            btnWipeDrive.Click += btnWipeDrive_Click;
            // 
            // trackBarBufferSize
            // 
            trackBarBufferSize.Location = new Point(193, 156);
            trackBarBufferSize.Maximum = 1024;
            trackBarBufferSize.Minimum = 64;
            trackBarBufferSize.Name = "trackBarBufferSize";
            trackBarBufferSize.Size = new Size(104, 45);
            trackBarBufferSize.TabIndex = 2;
            trackBarBufferSize.Value = 64;
            // 
            // labelBufferSize
            // 
            labelBufferSize.AutoSize = true;
            labelBufferSize.Location = new Point(204, 131);
            labelBufferSize.Name = "labelBufferSize";
            labelBufferSize.Size = new Size(19, 15);
            labelBufferSize.TabIndex = 3;
            labelBufferSize.Text = "64";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 389);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(347, 23);
            progressBar.TabIndex = 4;
            // 
            // logTextBox
            // 
            logTextBox.Location = new Point(12, 225);
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.Size = new Size(347, 144);
            logTextBox.TabIndex = 5;
            // 
            // numericPasses
            // 
            numericPasses.Location = new Point(229, 48);
            numericPasses.Name = "numericPasses";
            numericPasses.Size = new Size(92, 23);
            numericPasses.TabIndex = 6;
            // 
            // checkBoxQuick
            // 
            checkBoxQuick.AutoSize = true;
            checkBoxQuick.Location = new Point(204, 102);
            checkBoxQuick.Name = "checkBoxQuick";
            checkBoxQuick.Size = new Size(98, 19);
            checkBoxQuick.TabIndex = 7;
            checkBoxQuick.Text = "Quick Format";
            checkBoxQuick.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(79, 155);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // lblPasses
            // 
            lblPasses.AutoSize = true;
            lblPasses.Location = new Point(246, 20);
            lblPasses.Name = "lblPasses";
            lblPasses.Size = new Size(102, 15);
            lblPasses.TabIndex = 9;
            lblPasses.Text = "Number of passes";
            // 
            // lblDrive
            // 
            lblDrive.AutoSize = true;
            lblDrive.Location = new Point(79, 20);
            lblDrive.Name = "lblDrive";
            lblDrive.Size = new Size(76, 15);
            lblDrive.TabIndex = 10;
            lblDrive.Text = "Choose drive";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(373, 437);
            Controls.Add(lblDrive);
            Controls.Add(lblPasses);
            Controls.Add(btnCancel);
            Controls.Add(checkBoxQuick);
            Controls.Add(numericPasses);
            Controls.Add(logTextBox);
            Controls.Add(progressBar);
            Controls.Add(labelBufferSize);
            Controls.Add(trackBarBufferSize);
            Controls.Add(btnWipeDrive);
            Controls.Add(comboBoxDrive);
            Name = "Form1";
            Text = "Data Purger";
            ((System.ComponentModel.ISupportInitialize)trackBarBufferSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericPasses).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox comboBoxDrive;
        private Button btnWipeDrive;
        private TrackBar trackBarBufferSize;
        private Label labelBufferSize;
        private ProgressBar progressBar;
        private TextBox logTextBox;
        private NumericUpDown numericPasses;
        private CheckBox checkBoxQuick;
        private Button btnCancel;
        private Label lblPasses;
        private Label lblDrive;
    }
}
