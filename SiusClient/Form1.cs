using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SiusClient
{
    public partial class Form1 : Form
    {
        private readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private string _authToken = string.Empty;
        private Anlass _gewaehlterAnlass;
        private CancellationTokenSource _backgroundCts;
        private Task _backgroundTask = Task.CompletedTask;
        private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();
        private string _serverAdresse;
        private string _importPfad;
        private string _exportPfad;
        private long _importPosition;
        private bool _schliesst;
        private readonly Dictionary<string, string> _fehler = new Dictionary<string, string>();

        private readonly HashSet<long> _bereitsImportierteLogEvents = new HashSet<long>();
        private readonly HashSet<string> _bereitsImportierteZeilen = new HashSet<string>();


        private int _anzahlGeleseneSchussdaten = 0;
        private int _anzahlSchuetzen = 0;
        private int _hoechsteSchuetzennummer = 0;
        private string _uhrzeitLetzterSchuss = "-";
        private int _letzteServerShooterId = 0;

        public Form1()
        {
            InitializeComponent();

            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            btnOrdnerImport.Click += btnOrdnerImport_Click;
            btnOrdnerExport.Click += btnOrdnerExport_Click;
            btnVerbindungStarten.Click += btnVerbindungStarten_Click;
            btnAnlassWählen.Click += btnAnlassWählen_Click;
            btnNeuEinlesen.Click += btnNeuEinlesen_Click;

            tbPasswort.UseSystemPasswordChar = true;
            btnVerbindungStarten.UseVisualStyleBackColor = false;

            SetVerbindungsStatus(VerbindungsStatus.NichtVerbunden);
            UIStatistikenAktualisieren();
            SetEingabenAktiv(true);
        }

        public class Anlass
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public int ExpiresIn { get; set; }
        }

        public class ShooterDto
        {
            public int Id { get; set; }
            public int Startnummer { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Vorname { get; set; } = string.Empty;
            public string Verein { get; set; } = string.Empty;
            public int Bahn { get; set; }
            public int Abloesung { get; set; }
            public bool Aktiv { get; set; }
        }

        public class ShotImportDto
        {
            public int AnlassId { get; set; }
            public List<SiusShotRecord> Shots { get; set; } = new List<SiusShotRecord>();
        }

        public class SiusShotRecord
        {
            public int StartNr { get; set; }
            public decimal Primaerwertung { get; set; }
            public int Schussart { get; set; }
            public int BahnNr { get; set; }
            public decimal Sekundaerwertung { get; set; }
            public int Teiler { get; set; }
            public string Zeit { get; set; } = string.Empty;
            public int Mouche { get; set; }
            public decimal X { get; set; }
            public decimal Y { get; set; }
            public int InTime { get; set; }
            public decimal TimeSinceChange { get; set; }
            public int SweepDirection { get; set; }
            public int Demonstration { get; set; }
            public int Match { get; set; }
            public int Stich { get; set; }
            public int InsDel { get; set; }
            public int TotalArt { get; set; }
            public int Gruppe { get; set; }
            public int Feuerart { get; set; }
            public long LogEvent { get; set; }
            public int LogTyp { get; set; }
            public long ZeitSeitJahresbeginn { get; set; }
            public int Abloesung { get; set; }
            public int Waffe { get; set; }
            public int Position { get; set; }
            public int TargetId { get; set; }
            public int ExterneNummer { get; set; }
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

        private async void btnAnlassWählen_Click(object sender, EventArgs e)
        {
            await AnlassAuswaehlenAsync();
        }

        private async void btnNeuEinlesen_Click(object sender, EventArgs e)
        {
            SetEingabenAktiv(false);
            bool neustarten = _backgroundCts != null;
            await HintergrundprozessStoppenAsync();
            ImportZuruecksetzen();
            if (!_schliesst && neustarten) HintergrundprozessStarten();
            if (!_schliesst) SetEingabenAktiv(true);
        }

        private void ImportZuruecksetzen()
        {
            _importPosition = 0;
            _bereitsImportierteZeilen.Clear();
            _bereitsImportierteLogEvents.Clear();
            _anzahlGeleseneSchussdaten = 0;
            _uhrzeitLetzterSchuss = "-";

            UIStatistikenAktualisieren();

        }

        private async Task MitServerVerbindenAsync()
        {
            string serverAdresse = tbServerAdresse.Text.Trim().TrimEnd('/');
            string benutzername = tbBenutzername.Text.Trim();
            string passwort = tbPasswort.Text;

            if (!EingabenSindGueltig(serverAdresse, benutzername, passwort))
            {
                return;
            }

            try
            {
                SetEingabenAktiv(false);
                await HintergrundprozessStoppenAsync();
                _lifetimeCts.Token.ThrowIfCancellationRequested();
                _serverAdresse = serverAdresse;
                _gewaehlterAnlass = null;
                cbAnlass.DataSource = null;
                _authToken = string.Empty;
                _httpClient.DefaultRequestHeaders.Authorization = null;
                SetVerbindungsStatus(VerbindungsStatus.Verbindet);

                string loginUrl = $"{serverAdresse}/api/login";

                var loginRequest = new LoginRequest
                {
                    Username = benutzername,
                    Password = passwort
                };

                string json = JsonSerializer.Serialize(loginRequest);
                string responseText;
                using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                using (var response = await _httpClient.PostAsync(loginUrl, content, _lifetimeCts.Token))
                {
                    response.EnsureSuccessStatusCode();
                    responseText = await response.Content.ReadAsStringAsync();
                }
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseText, _jsonOptions);
                if (string.IsNullOrWhiteSpace(loginResponse?.Token))
                    throw new InvalidDataException("Der Server hat kein gültiges Login-Token geliefert.");
                _authToken = loginResponse.Token;

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authToken);

                await AnlaesseVomServerLadenAsync();
                _lifetimeCts.Token.ThrowIfCancellationRequested();
                SetVerbindungsStatus(VerbindungsStatus.Verbunden);

                MessageBox.Show(
                    "Verbindung erfolgreich hergestellt und Anlässe geladen.",
                    "Erfolg",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _authToken = string.Empty;
                _httpClient.DefaultRequestHeaders.Authorization = null;
                if (_schliesst) return;
                SetVerbindungsStatus(VerbindungsStatus.NichtVerbunden);

                MessageBox.Show(
                    "Fehler bei der Serververbindung:\n\n" + ex.Message,
                    "Verbindungsfehler",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                if (!_schliesst) SetEingabenAktiv(true);
            }
        }

        private async Task AnlaesseVomServerLadenAsync()
        {
            string serverAdresse = _serverAdresse;
            string url = $"{serverAdresse}/api/anlaesse";

            var anlaesse = await GetJsonAsync<List<Anlass>>(url, _lifetimeCts.Token);

            cbAnlass.DataSource = null;
            cbAnlass.DataSource = anlaesse;
            cbAnlass.DisplayMember = "Name";
            cbAnlass.ValueMember = "Id";
        }

        private async Task AnlassAuswaehlenAsync()
        {
            var anlass = cbAnlass.SelectedItem as Anlass;
            if (anlass == null || string.IsNullOrWhiteSpace(_authToken))
            {
                MessageBox.Show("Bitte zuerst verbinden und einen Anlass auswählen.");
                return;
            }

            try
            {
                SetEingabenAktiv(false);
                await HintergrundprozessStoppenAsync();
                _lifetimeCts.Token.ThrowIfCancellationRequested();
                _gewaehlterAnlass = null;
                _importPfad = Path.GetFullPath(tbImportDatei.Text.Trim());
                _exportPfad = Path.GetFullPath(tbExportDatei.Text.Trim());
                if (string.Equals(_importPfad, _exportPfad, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Import- und Exportdatei müssen verschieden sein.");

                await SchuetzenVomServerExportierenAsync(anlass.Id);
                _lifetimeCts.Token.ThrowIfCancellationRequested();
                ImportZuruecksetzen();
                _fehler.Clear();
                _gewaehlterAnlass = anlass;
                lblAnlass.Text = anlass.Name;
                HintergrundprozessStarten();
            }
            catch (Exception ex)
            {
                if (!_schliesst)
                    MessageBox.Show("Anlass konnte nicht gestartet werden:\n\n" + ex.Message,
                        "Exportfehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!_schliesst) SetEingabenAktiv(true);
            }
        }

        private async Task<T> GetJsonAsync<T>(string url, CancellationToken cancellationToken) where T : class
        {
            using (var response = await _httpClient.GetAsync(url, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                cancellationToken.ThrowIfCancellationRequested();
                return JsonSerializer.Deserialize<T>(json, _jsonOptions)
                    ?? throw new InvalidDataException("Der Server hat eine leere JSON-Antwort geliefert.");
            }
        }

        private static string ShooterCsv(ShooterDto shooter)
        {
            if (shooter == null) throw new InvalidDataException("Die Schützenliste enthält einen leeren Eintrag.");
            string vorname = (shooter.Vorname ?? "").Trim();
            string anzeigename = vorname.Length == 0 ? shooter.Name : vorname[0] + ". " + shooter.Name;
            return string.Join(",", shooter.Id.ToString(CultureInfo.InvariantCulture),
                CsvEscape(shooter.Name), CsvEscape(vorname), CsvEscape(anzeigename),
                "", "", "", "", "", "", "");
        }

        private async Task SchuetzenVomServerExportierenAsync(int anlassId)
        {
            var shooters = await GetJsonAsync<List<ShooterDto>>(
                $"{_serverAdresse}/api/anlaesse/{anlassId}/shooters", _lifetimeCts.Token);
            var zeilen = new List<string> { "0,DummyName,Dummy,D. DummyName,,,,,,," };
            zeilen.AddRange(shooters.Select(ShooterCsv));
            await DateiSchreibenAsync(_exportPfad, zeilen, false, _lifetimeCts.Token);
            _anzahlSchuetzen = shooters.Count;
            _hoechsteSchuetzennummer = shooters.Count == 0 ? 0 : shooters.Max(x => x.Startnummer);
            _letzteServerShooterId = shooters.Count == 0 ? 0 : shooters.Max(x => x.Id);
            UIStatistikenAktualisieren();
        }

        private void HintergrundprozessStarten()
        {
            _backgroundCts = CancellationTokenSource.CreateLinkedTokenSource(_lifetimeCts.Token);
            _backgroundTask = HintergrundLoopAsync(_backgroundCts.Token);
        }

        private async Task HintergrundprozessStoppenAsync()
        {
            var cts = _backgroundCts;
            if (cts == null) return;
            cts.Cancel();
            try { await _backgroundTask; }
            catch (OperationCanceledException) { }
            finally
            {
                cts.Dispose();
                _backgroundCts = null;
            }
        }

        private async Task HintergrundLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Ein Importfehler darf den unabhängigen Schützenexport nicht blockieren.
                    await HintergrundSchrittAsync("Schussimport", ImportDateiPruefenUndSendenAsync, cancellationToken);
                    await HintergrundSchrittAsync("Schützenexport", NeueSchuetzenVomServerPruefenAsync, cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        }

        private async Task HintergrundSchrittAsync(string bereich,
            Func<CancellationToken, Task> aktion, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await aktion(cancellationToken);
                _fehler.Remove(bereich);
                Text = _fehler.Count == 0 ? "SiusClient" : "SiusClient – " + string.Join(" | ", _fehler.Values);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                if (_schliesst) return;
                string meldung = bereich + ": " + (ex is OperationCanceledException
                    ? "Zeitüberschreitung beim Serverzugriff." : ex.Message);
                // Nichtmodal: keine Meldungsflut im 3-Sekunden-Takt.
                _fehler[bereich] = meldung;
                Text = "SiusClient – " + string.Join(" | ", _fehler.Values);
                System.Diagnostics.Trace.TraceError(meldung);
            }
        }

        private sealed class ImportBlock
        {
            public long Position;
            public List<string> Zeilen = new List<string>();
        }

        private static ImportBlock ImportBlockLesen(string pfad, long position)
        {
            using (var stream = new FileStream(pfad, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete))
            {
                if (stream.Length < position) position = 0;
                stream.Position = position;
                var block = new ImportBlock { Position = position };
                var zeile = new List<byte>();
                // ISO-8859-1: ein Byte pro Zeichen. Nur vollständige Zeilen quittieren.
                byte[] buffer = new byte[64 * 1024];
                int anzahl = stream.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < anzahl; i++)
                {
                    if (buffer[i] == (byte)'\n')
                    {
                        block.Zeilen.Add(Encoding.GetEncoding("ISO-8859-1")
                            .GetString(zeile.ToArray()).TrimEnd('\r'));
                        zeile.Clear();
                        block.Position = position + i + 1;
                        if (block.Zeilen.Count == 500) break;
                    }
                    else zeile.Add(buffer[i]);
                }
                if (block.Zeilen.Count == 0 && anzahl == buffer.Length)
                    throw new InvalidDataException("Importzeile überschreitet 64 KiB oder enthält keinen Zeilenumbruch.");
                return block;
            }
        }

        private async Task ImportDateiPruefenUndSendenAsync(CancellationToken cancellationToken)
        {
            var block = await Task.Run(() => ImportBlockLesen(_importPfad, _importPosition), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var shots = new List<SiusShotRecord>();
            var zeilen = new HashSet<string>();
            var events = new HashSet<long>();
            foreach (string zeile in block.Zeilen)
            {
                if (string.IsNullOrWhiteSpace(zeile) || _bereitsImportierteZeilen.Contains(zeile)
                    || !zeilen.Add(zeile)) continue;
                if (!TryParseSiusShotRecord(zeile, out var shot))
                    throw new InvalidDataException(
                        $"Ungültige SIUS-Daten im Block ab Byte {_importPosition}. Import angehalten; bitte CSV korrigieren.");
                if (shot.LogEvent > 0 &&
                    (_bereitsImportierteLogEvents.Contains(shot.LogEvent) || !events.Add(shot.LogEvent))) continue;
                shots.Add(shot);
            }

            if (shots.Count > 0)
            {
                var payload = new ShotImportDto { AnlassId = _gewaehlterAnlass.Id, Shots = shots };
                using (var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"))
                using (var response = await _httpClient.PostAsync(
                    $"{_serverAdresse}/api/anlaesse/{payload.AnlassId}/shots/import", content, cancellationToken))
                {
                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();
                    using (var result = JsonDocument.Parse(json))
                    {
                        if (!result.RootElement.TryGetProperty("success", out var success)
                            || success.ValueKind != JsonValueKind.True)
                            throw new InvalidDataException("Der Server hat den Schussimport nicht bestätigt.");
                    }
                }
            }

            // Erst nach bestätigter Übertragung fortschreiten, sonst denselben Block erneut senden.
            _bereitsImportierteZeilen.UnionWith(zeilen);
            _bereitsImportierteLogEvents.UnionWith(events);
            _importPosition = block.Position;
            _anzahlGeleseneSchussdaten += shots.Count;
            if (shots.Count > 0) _uhrzeitLetzterSchuss = shots[shots.Count - 1].Zeit;
            UIStatistikenAktualisieren();
        }

        private async Task NeueSchuetzenVomServerPruefenAsync(CancellationToken cancellationToken)
        {
            var neueShooter = await GetJsonAsync<List<ShooterDto>>(
                $"{_serverAdresse}/api/anlaesse/{_gewaehlterAnlass.Id}/shooters/new?sinceId={_letzteServerShooterId}",
                cancellationToken);
            if (neueShooter.Any(s => s == null))
                throw new InvalidDataException("Die Schützenliste enthält einen leeren Eintrag.");
            neueShooter = neueShooter.Where(s => s.Id > _letzteServerShooterId)
                .GroupBy(s => s.Id).Select(g => g.First()).ToList();
            if (neueShooter.Count == 0) return;

            await DateiSchreibenAsync(_exportPfad, neueShooter.Select(ShooterCsv).ToList(), true, cancellationToken);
            _anzahlSchuetzen += neueShooter.Count;
            _hoechsteSchuetzennummer = Math.Max(_hoechsteSchuetzennummer, neueShooter.Max(x => x.Startnummer));
            _letzteServerShooterId = neueShooter.Max(x => x.Id);
            UIStatistikenAktualisieren();
        }
        private bool TryParseSiusShotRecord(string zeile, out SiusShotRecord shot)
        {
            shot = new SiusShotRecord();

            // SIUS Schussformat hat 28 Felder.
            // Reihenfolge gemäss Doku: StartNr, Primärwertung, Schussart, Bahn Nr, Sekundärwertung, Teiler, Zeit, ...

            string[] teile = zeile.Split(';');

            if (teile.Length < 28)
                return false;

            try
            {
                shot.StartNr = ParseInt(teile[0]);
                shot.Primaerwertung = ParseDecimal(teile[1]);
                shot.Schussart = ParseInt(teile[2]);
                shot.BahnNr = ParseInt(teile[3]);
                shot.Sekundaerwertung = ParseDecimal(teile[4]);
                shot.Teiler = ParseInt(teile[5]);
                shot.Zeit = teile[6];
                shot.Mouche = ParseInt(teile[7]);
                shot.X = ParseDecimal(teile[8]);
                shot.Y = ParseDecimal(teile[9]);
                shot.InTime = ParseInt(teile[10]);
                shot.TimeSinceChange = ParseDecimal(teile[11]);
                shot.SweepDirection = ParseInt(teile[12]);
                shot.Demonstration = ParseInt(teile[13]);
                shot.Match = ParseInt(teile[14]);
                shot.Stich = ParseInt(teile[15]);
                shot.InsDel = ParseInt(teile[16]);
                shot.TotalArt = ParseInt(teile[17]);
                shot.Gruppe = ParseInt(teile[18]);
                shot.Feuerart = ParseInt(teile[19]);
                shot.LogEvent = ParseLong(teile[20]);
                shot.LogTyp = ParseInt(teile[21]);
                shot.ZeitSeitJahresbeginn = ParseLong(teile[22]);
                shot.Abloesung = ParseInt(teile[23]);
                shot.Waffe = ParseInt(teile[24]);
                shot.Position = ParseInt(teile[25]);
                shot.TargetId = ParseInt(teile[26]);
                shot.ExterneNummer = ParseInt(teile[27]);

                return true;
            }
            catch (FormatException) { return false; }
            catch (OverflowException) { return false; }
        }

        private static int ParseInt(string value)
        {
            return int.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }

        private static long ParseLong(string value)
        {
            return long.Parse(value.Trim(), CultureInfo.InvariantCulture);
        }

        private static decimal ParseDecimal(string value)
        {
            value = value.Trim().Replace(",", ".");
            return decimal.Parse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            value = value.Trim();
            return value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0
                ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
        }

        private bool EingabenSindGueltig(string serverAdresse, string benutzername, string passwort)
        {
            if (string.IsNullOrWhiteSpace(serverAdresse))
            {
                MessageBox.Show("Bitte Server-Adresse eingeben.");
                return false;
            }

            if (!Uri.TryCreate(serverAdresse, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query)
                || !string.IsNullOrEmpty(uri.Fragment))
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

        private void UIStatistikenAktualisieren()
        {
            if (_schliesst || IsDisposed) return;
            lblAnzahlSchuezen.Text = _anzahlSchuetzen.ToString();
            lblHoechsteSchuezennummer.Text = _hoechsteSchuetzennummer.ToString();
            lblAnzahlGeleseneSchussdaten.Text = _anzahlGeleseneSchussdaten.ToString();
            lblUhrzeitLetzterSchuss.Text = _uhrzeitLetzterSchuss;
        }

        private static async Task DateiSchreibenAsync(string pfad, List<string> zeilen,
            bool anhaengen, CancellationToken cancellationToken)
        {
            // Austausch im selben Verzeichnis: Leser sehen die alte oder die vollständige neue Datei.
            for (int versuch = 0; ; versuch++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await Task.Run(() =>
                    {
                        string temporaer = pfad + "." + Guid.NewGuid().ToString("N") + ".tmp";
                        try
                        {
                            var encoding = Encoding.GetEncoding("ISO-8859-1",
                                EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
                            using (var output = new FileStream(temporaer, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                            {
                                if (anhaengen)
                                {
                                    using (var input = new FileStream(pfad, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        input.CopyTo(output);
                                        if (input.Length > 0)
                                        {
                                            input.Position = input.Length - 1;
                                            if (input.ReadByte() != '\n') output.WriteByte((byte)'\n');
                                        }
                                    }
                                }
                                using (var writer = new StreamWriter(output, encoding, 4096, true))
                                    foreach (string zeile in zeilen) writer.WriteLine(zeile);
                                output.Flush(true);
                            }
                            cancellationToken.ThrowIfCancellationRequested();
                            if (File.Exists(pfad)) File.Replace(temporaer, pfad, null);
                            else File.Move(temporaer, pfad);
                        }
                        finally
                        {
                            try { if (File.Exists(temporaer)) File.Delete(temporaer); }
                            catch (IOException) { }
                            catch (UnauthorizedAccessException) { }
                        }
                    }, cancellationToken);
                    return;
                }
                catch (IOException ex) when (versuch < 4 &&
                    ((ex.HResult & 0xffff) == 32 || (ex.HResult & 0xffff) == 33))
                {
                    await Task.Delay(100 + versuch * 100, cancellationToken);
                }
            }
        }

        private void SetEingabenAktiv(bool aktiv)
        {
            btnVerbindungStarten.Enabled = aktiv;
            btnAnlassWählen.Enabled = aktiv && !string.IsNullOrWhiteSpace(_authToken);
            btnNeuEinlesen.Enabled = aktiv;
            cbAnlass.Enabled = aktiv;
            bool konfiguration = aktiv && _backgroundCts == null;
            tbImportDatei.ReadOnly = !konfiguration;
            tbExportDatei.ReadOnly = !konfiguration;
            tbServerAdresse.ReadOnly = !konfiguration;
            tbBenutzername.ReadOnly = !konfiguration;
            tbPasswort.ReadOnly = !konfiguration;
            btnOrdnerImport.Enabled = konfiguration;
            btnOrdnerExport.Enabled = konfiguration;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (e.Cancel) return;
            _schliesst = true;
            _lifetimeCts.Cancel();
            _backgroundCts?.Cancel();
            _httpClient.Dispose();
        }
    }
    public enum VerbindungsStatus
    {
        NichtVerbunden,
        Verbindet,
        Verbunden
    }
}
