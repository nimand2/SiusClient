namespace SiusClient
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.tbExportDatei = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnOrdnerImport = new System.Windows.Forms.Button();
            this.btnOrdnerExport = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbImportDatei = new System.Windows.Forms.TextBox();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.panel11 = new System.Windows.Forms.Panel();
            this.btnAnlassWählen = new System.Windows.Forms.Button();
            this.btnVerbindungStarten = new System.Windows.Forms.Button();
            this.tbPasswort = new System.Windows.Forms.TextBox();
            this.tbBenutzername = new System.Windows.Forms.TextBox();
            this.tbServerAdresse = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl4 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblAnzahlSchuezen = new System.Windows.Forms.Label();
            this.lblHoechsteSchuezennummer = new System.Windows.Forms.Label();
            this.lblAnzahlGeleseneSchussdaten = new System.Windows.Forms.Label();
            this.lblUhrzeitLetzterSchuss = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblAnlass = new System.Windows.Forms.Label();
            this.cbAnlass = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.panel11.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbExportDatei
            // 
            this.tbExportDatei.Location = new System.Drawing.Point(118, 55);
            this.tbExportDatei.Name = "tbExportDatei";
            this.tbExportDatei.Size = new System.Drawing.Size(540, 20);
            this.tbExportDatei.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnOrdnerImport);
            this.panel1.Controls.Add(this.btnOrdnerExport);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.tbImportDatei);
            this.panel1.Controls.Add(this.lbl2);
            this.panel1.Controls.Add(this.lbl1);
            this.panel1.Controls.Add(this.tbExportDatei);
            this.panel1.Location = new System.Drawing.Point(11, 10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(778, 134);
            this.panel1.TabIndex = 2;
            // 
            // btnOrdnerImport
            // 
            this.btnOrdnerImport.Location = new System.Drawing.Point(664, 85);
            this.btnOrdnerImport.Name = "btnOrdnerImport";
            this.btnOrdnerImport.Size = new System.Drawing.Size(113, 20);
            this.btnOrdnerImport.TabIndex = 7;
            this.btnOrdnerImport.Text = "Ordner";
            this.btnOrdnerImport.UseVisualStyleBackColor = true;
            // 
            // btnOrdnerExport
            // 
            this.btnOrdnerExport.Location = new System.Drawing.Point(664, 55);
            this.btnOrdnerExport.Name = "btnOrdnerExport";
            this.btnOrdnerExport.Size = new System.Drawing.Size(113, 20);
            this.btnOrdnerExport.TabIndex = 4;
            this.btnOrdnerExport.Text = "Ordner";
            this.btnOrdnerExport.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(7, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "Import Datei :";
            // 
            // tbImportDatei
            // 
            this.tbImportDatei.Location = new System.Drawing.Point(118, 85);
            this.tbImportDatei.Name = "tbImportDatei";
            this.tbImportDatei.Size = new System.Drawing.Size(540, 20);
            this.tbImportDatei.TabIndex = 5;
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl2.Location = new System.Drawing.Point(7, 55);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(105, 20);
            this.lbl2.TabIndex = 3;
            this.lbl2.Text = "Export Datei :";
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl1.Location = new System.Drawing.Point(7, 7);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(232, 20);
            this.lbl1.TabIndex = 2;
            this.lbl1.Text = "Sius Daten Expoprt / Import";
            // 
            // panel11
            // 
            this.panel11.Controls.Add(this.cbAnlass);
            this.panel11.Controls.Add(this.lblAnlass);
            this.panel11.Controls.Add(this.btnAnlassWählen);
            this.panel11.Controls.Add(this.btnVerbindungStarten);
            this.panel11.Controls.Add(this.tbPasswort);
            this.panel11.Controls.Add(this.tbBenutzername);
            this.panel11.Controls.Add(this.tbServerAdresse);
            this.panel11.Controls.Add(this.label4);
            this.panel11.Controls.Add(this.label3);
            this.panel11.Controls.Add(this.label2);
            this.panel11.Controls.Add(this.lbl4);
            this.panel11.Location = new System.Drawing.Point(11, 154);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(777, 182);
            this.panel11.TabIndex = 3;
            // 
            // btnAnlassWählen
            // 
            this.btnAnlassWählen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnlassWählen.Location = new System.Drawing.Point(511, 100);
            this.btnAnlassWählen.Name = "btnAnlassWählen";
            this.btnAnlassWählen.Size = new System.Drawing.Size(241, 52);
            this.btnAnlassWählen.TabIndex = 11;
            this.btnAnlassWählen.Text = "Anlass Wählen";
            this.btnAnlassWählen.UseVisualStyleBackColor = true;
            // 
            // btnVerbindungStarten
            // 
            this.btnVerbindungStarten.BackColor = System.Drawing.Color.Red;
            this.btnVerbindungStarten.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnVerbindungStarten.Location = new System.Drawing.Point(511, 45);
            this.btnVerbindungStarten.Name = "btnVerbindungStarten";
            this.btnVerbindungStarten.Size = new System.Drawing.Size(241, 52);
            this.btnVerbindungStarten.TabIndex = 10;
            this.btnVerbindungStarten.Text = "Verbindung Starten";
            this.btnVerbindungStarten.UseVisualStyleBackColor = false;
            // 
            // tbPasswort
            // 
            this.tbPasswort.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbPasswort.Location = new System.Drawing.Point(131, 100);
            this.tbPasswort.Name = "tbPasswort";
            this.tbPasswort.Size = new System.Drawing.Size(363, 24);
            this.tbPasswort.TabIndex = 9;
            // 
            // tbBenutzername
            // 
            this.tbBenutzername.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbBenutzername.Location = new System.Drawing.Point(131, 73);
            this.tbBenutzername.Name = "tbBenutzername";
            this.tbBenutzername.Size = new System.Drawing.Size(363, 24);
            this.tbBenutzername.TabIndex = 8;
            // 
            // tbServerAdresse
            // 
            this.tbServerAdresse.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbServerAdresse.Location = new System.Drawing.Point(131, 46);
            this.tbServerAdresse.Name = "tbServerAdresse";
            this.tbServerAdresse.Size = new System.Drawing.Size(363, 24);
            this.tbServerAdresse.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(8, 99);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 18);
            this.label4.TabIndex = 6;
            this.label4.Text = "Passwort :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 18);
            this.label3.TabIndex = 5;
            this.label3.Text = "Benutzername :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(117, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "Server Adresse :";
            // 
            // lbl4
            // 
            this.lbl4.AutoSize = true;
            this.lbl4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl4.Location = new System.Drawing.Point(7, 13);
            this.lbl4.Name = "lbl4";
            this.lbl4.Size = new System.Drawing.Size(158, 20);
            this.lbl4.TabIndex = 3;
            this.lbl4.Text = "Server Verbindung";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 34.53608F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 65.46392F));
            this.tableLayoutPanel1.Controls.Add(this.label6, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblAnzahlSchuezen, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblHoechsteSchuezennummer, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblAnzahlGeleseneSchussdaten, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblUhrzeitLetzterSchuss, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(11, 348);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(776, 107);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(3, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(221, 20);
            this.label6.TabIndex = 1;
            this.label6.Text = "Höchste Schützennummer";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(3, 54);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(252, 20);
            this.label7.TabIndex = 2;
            this.label7.Text = "Anzahl gelesene Schussdaten";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(3, 80);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(187, 20);
            this.label8.TabIndex = 3;
            this.label8.Text = "Uhrzeit Lezter Schuss";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAnzahlSchuezen
            // 
            this.lblAnzahlSchuezen.AutoSize = true;
            this.lblAnzahlSchuezen.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnzahlSchuezen.Location = new System.Drawing.Point(270, 0);
            this.lblAnzahlSchuezen.Name = "lblAnzahlSchuezen";
            this.lblAnzahlSchuezen.Size = new System.Drawing.Size(13, 20);
            this.lblAnzahlSchuezen.TabIndex = 4;
            this.lblAnzahlSchuezen.Text = ".";
            this.lblAnzahlSchuezen.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblHoechsteSchuezennummer
            // 
            this.lblHoechsteSchuezennummer.AutoSize = true;
            this.lblHoechsteSchuezennummer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHoechsteSchuezennummer.Location = new System.Drawing.Point(270, 27);
            this.lblHoechsteSchuezennummer.Name = "lblHoechsteSchuezennummer";
            this.lblHoechsteSchuezennummer.Size = new System.Drawing.Size(13, 20);
            this.lblHoechsteSchuezennummer.TabIndex = 5;
            this.lblHoechsteSchuezennummer.Text = ".";
            this.lblHoechsteSchuezennummer.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAnzahlGeleseneSchussdaten
            // 
            this.lblAnzahlGeleseneSchussdaten.AutoSize = true;
            this.lblAnzahlGeleseneSchussdaten.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnzahlGeleseneSchussdaten.Location = new System.Drawing.Point(270, 54);
            this.lblAnzahlGeleseneSchussdaten.Name = "lblAnzahlGeleseneSchussdaten";
            this.lblAnzahlGeleseneSchussdaten.Size = new System.Drawing.Size(13, 20);
            this.lblAnzahlGeleseneSchussdaten.TabIndex = 6;
            this.lblAnzahlGeleseneSchussdaten.Text = ".";
            this.lblAnzahlGeleseneSchussdaten.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblUhrzeitLetzterSchuss
            // 
            this.lblUhrzeitLetzterSchuss.AutoSize = true;
            this.lblUhrzeitLetzterSchuss.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUhrzeitLetzterSchuss.Location = new System.Drawing.Point(270, 80);
            this.lblUhrzeitLetzterSchuss.Name = "lblUhrzeitLetzterSchuss";
            this.lblUhrzeitLetzterSchuss.Size = new System.Drawing.Size(13, 20);
            this.lblUhrzeitLetzterSchuss.TabIndex = 7;
            this.lblUhrzeitLetzterSchuss.Text = ".";
            this.lblUhrzeitLetzterSchuss.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(145, 20);
            this.label5.TabIndex = 0;
            this.label5.Text = "Anzahl Schützen";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAnlass
            // 
            this.lblAnlass.AutoSize = true;
            this.lblAnlass.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAnlass.Location = new System.Drawing.Point(8, 130);
            this.lblAnlass.Name = "lblAnlass";
            this.lblAnlass.Size = new System.Drawing.Size(60, 18);
            this.lblAnlass.TabIndex = 12;
            this.lblAnlass.Text = "Anlass :";
            // 
            // cbAnlass
            // 
            this.cbAnlass.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbAnlass.FormattingEnabled = true;
            this.cbAnlass.Location = new System.Drawing.Point(131, 131);
            this.cbAnlass.Name = "cbAnlass";
            this.cbAnlass.Size = new System.Drawing.Size(363, 28);
            this.cbAnlass.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 467);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel11);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "SiusClient";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel11.ResumeLayout(false);
            this.panel11.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox tbExportDatei;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Button btnOrdnerImport;
        private System.Windows.Forms.Button btnOrdnerExport;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbImportDatei;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.Panel panel11;
        private System.Windows.Forms.Label lbl4;
        private System.Windows.Forms.TextBox tbPasswort;
        private System.Windows.Forms.TextBox tbBenutzername;
        private System.Windows.Forms.TextBox tbServerAdresse;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnVerbindungStarten;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label lblAnzahlSchuezen;
        private System.Windows.Forms.Label lblHoechsteSchuezennummer;
        private System.Windows.Forms.Label lblAnzahlGeleseneSchussdaten;
        private System.Windows.Forms.Label lblUhrzeitLetzterSchuss;
        private System.Windows.Forms.Button btnAnlassWählen;
        private System.Windows.Forms.Label lblAnlass;
        private System.Windows.Forms.ComboBox cbAnlass;
    }
}

