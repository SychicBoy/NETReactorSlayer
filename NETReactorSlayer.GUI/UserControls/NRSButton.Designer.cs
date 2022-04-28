namespace NETReactorSlayer.GUI.UserControls
{
    partial class NrsButton
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.SuspendLayout();
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.Name = "RoundedButton";
            this.Size = new System.Drawing.Size(262, 86);
            this.BackColor = System.Drawing.Color.FromArgb(32,32,32);
            this.ForeColor = System.Drawing.Color.Silver;
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FlatAppearance.BorderColor = this.BackColor;
            this.FlatAppearance.BorderSize = 0;
            this.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(18,18,18);
            this.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(34, 34, 34);
            this.SizeChanged += new System.EventHandler(this.SizeChange);
            this.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ResumeLayout(false);
        }

        #endregion
    }
}
