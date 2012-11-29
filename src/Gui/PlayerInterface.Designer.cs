namespace globalwaves.Player.Gui
{
    partial class PlayerInterface
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
            this.components = new System.ComponentModel.Container();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.txtStatus = new System.Windows.Forms.Label();
            this.waveformTimer = new System.Windows.Forms.Timer(this.components);
            this.waveformL = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.waveformL)).BeginInit();
            this.SuspendLayout();
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(368, 200);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(48, 24);
            this.btnPlay.TabIndex = 0;
            this.btnPlay.Text = "Play";
            this.btnPlay.UseVisualStyleBackColor = true;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(422, 200);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(48, 24);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // txtStatus
            // 
            this.txtStatus.AutoSize = true;
            this.txtStatus.Location = new System.Drawing.Point(12, 206);
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.Size = new System.Drawing.Size(42, 13);
            this.txtStatus.TabIndex = 2;
            this.txtStatus.Text = "Ready.";
            // 
            // waveformTimer
            // 
            this.waveformTimer.Enabled = true;
            this.waveformTimer.Interval = 10;
            this.waveformTimer.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // waveformL
            // 
            this.waveformL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.waveformL.Location = new System.Drawing.Point(12, 12);
            this.waveformL.Name = "waveformL";
            this.waveformL.Size = new System.Drawing.Size(458, 182);
            this.waveformL.TabIndex = 3;
            this.waveformL.TabStop = false;
            // 
            // PlayerInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(482, 236);
            this.Controls.Add(this.waveformL);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnPlay);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.Name = "PlayerInterface";
            this.Text = "PlayerInterface";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PlayerInterface_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.waveformL)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label txtStatus;
        private System.Windows.Forms.Timer waveformTimer;
        private System.Windows.Forms.PictureBox waveformL;
    }
}