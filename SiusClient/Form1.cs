using System;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiusClient
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string _authToken = string.Empty;

        public Form1()
        {
            InitializeComponent();

            btnOrdnerImport.Click += btnOrdnerImport_Click;
            btnOrdnerExport.Click += btnOrdnerExport_Click;

            tbPasswort.UseSystemPasswordChar = true;
            btnVerbindungStarten.UseVisualStyleBackColor = false;

            SetVerbindungsStatus(VerbindungsStatus.NichtVerbunden);

            btnVerbindungStarten.Click += btnVerbindungStarten_Click;

           
            TestAnlaesseLaden();
        }

        public class Anlass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private void TestAnlaesseLaden()
        {
            var anlaesse = new[]
            {
                new Anlass { Id = 1, Name = "Feldschiessen 2026" },
                new Anlass { Id = 2, Name = "Chilbischiessen Wangen" },
                new Anlass { Id = 3, Name = "Volksschiessen Lachen" }
            };

            cbAnlass.DataSource = anlaesse;
            cbAnlass.DisplayMember = "Name";
            cbAnlass.ValueMember = "Id";
        }

        private void btnOrdnerImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Import-Datei auswählen";
                ofd.Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.Multiselect = false;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbImportDatei.Text = ofd.FileName;                    
                }
            }
        }

        private void btnOrdnerExport_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Export-Datei auswählen";
                sfd.Filter = "CSV-Dateien (*.csv)|*.csv|Alle Dateien (*.*)|*.*";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                sfd.FileName = "Shooters.csv";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    tbExportDatei.Text = sfd.FileName;    
                }
            }
        }

        private async void btnVerbindungStarten_Click(object sender, EventArgs e)
        {
            await MitServerVerbindenAsync();
        }

        private async Task MitServerVerbindenAsync()
        {
            string serverAdresse = tbServerAdresse.Text.Trim();
            string benutzername = tbBenutzername.Text.Trim();
            string passwort = tbPasswort.Text;

            if (!EingabenSindGueltig(serverAdresse, benutzername, passwort))
            {
                return;
            }

            try
            {
                btnVerbindungStarten.Enabled = false;
                SetVerbindungsStatus(VerbindungsStatus.Verbindet);

                string loginUrl = serverAdresse + "/api/login";

                var loginRequest = new LoginRequest
                {
                    Username = benutzername,
                    Password = passwort
                };

                string json = JsonSerializer.Serialize(loginRequest);

                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    HttpResponseMessage response = await _httpClient.PostAsync(loginUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        _authToken = await response.Content.ReadAsStringAsync();
                        SetVerbindungsStatus(VerbindungsStatus.Verbunden);

                        MessageBox.Show(
                            "Verbindung erfolgreich hergestellt.",
                            "Erfolg",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        SetVerbindungsStatus(VerbindungsStatus.NichtVerbunden);

                        string fehlerText = await response.Content.ReadAsStringAsync();

                        MessageBox.Show(
                            "Login fehlgeschlagen.\n\n" +
                            "Statuscode: " + (int)response.StatusCode + "\n" +
                            "Antwort: " + fehlerText,
                            "Fehler",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                SetVerbindungsStatus(VerbindungsStatus.NichtVerbunden);

                MessageBox.Show(
                    "Fehler bei der Serververbindung:\n\n" + ex.Message,
                    "Verbindungsfehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnVerbindungStarten.Enabled = true;
            }
        }

        private bool EingabenSindGueltig(string serverAdresse, string benutzername, string passwort)
        {
            if (string.IsNullOrWhiteSpace(serverAdresse))
            {
                MessageBox.Show("Bitte Server-Adresse eingeben.");
                return false;
            }

            if (!serverAdresse.StartsWith("http://") && !serverAdresse.StartsWith("https://"))
            {
                MessageBox.Show("Bitte gib die Server-Adresse mit http:// oder https:// ein.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(benutzername))
            {
                MessageBox.Show("Bitte Benutzernamen eingeben.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(passwort))
            {
                MessageBox.Show("Bitte Passwort eingeben.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(tbImportDatei.Text.Trim()))
            {
                MessageBox.Show("Bitte zuerst eine Import-Datei auswählen.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(tbExportDatei.Text.Trim()))
            {
                MessageBox.Show("Bitte zuerst eine Export-Datei auswählen.");
                return false;
            }

            return true;
        }

        private void SetVerbindungsStatus(VerbindungsStatus status)
        {
            switch (status)
            {
                case VerbindungsStatus.NichtVerbunden:
                    btnVerbindungStarten.BackColor = Color.Red;
                    btnVerbindungStarten.Text = "Verbindung Starten";
                    break;

                case VerbindungsStatus.Verbindet:
                    btnVerbindungStarten.BackColor = Color.Orange;
                    btnVerbindungStarten.Text = "Verbinde...";
                    break;

                case VerbindungsStatus.Verbunden:
                    btnVerbindungStarten.BackColor = Color.LightGreen;
                    btnVerbindungStarten.Text = "Verbunden";
                    break;
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }

    public enum VerbindungsStatus
    {
        NichtVerbunden,
        Verbindet,
        Verbunden
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}