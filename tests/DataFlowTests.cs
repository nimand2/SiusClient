using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiusClient;

internal static class DataFlowTests
{
    private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    private static int checks;
    private static object Call(object target, string name, params object[] args)
    {
        try { return typeof(Form1).GetMethod(name, Flags).Invoke(target, args); }
        catch (TargetInvocationException ex) { throw ex.InnerException; }
    }
    private static void Set(Form1 form, string name, object value)
        => typeof(Form1).GetField(name, Flags).SetValue(form, value);
    private static T Get<T>(object obj, string name)
        => (T)obj.GetType().GetField(name, Flags | BindingFlags.Public).GetValue(obj);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        checks++;
    }
    private static string Shot(long id)
    {
        var fields = Enumerable.Repeat("0", 28).ToArray();
        fields[0] = "12";
        fields[1] = "10,5";
        fields[6] = "12:34:56";
        fields[20] = id.ToString();
        return string.Join(";", fields);
    }
    private sealed class Handler : HttpMessageHandler
    {
        public HttpStatusCode Status = HttpStatusCode.OK;
        public string Body = "{\"success\":true,\"imported\":1}";
        public int Requests;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Requests++;
            return Task.FromResult(new HttpResponseMessage(Status) { Content = new StringContent(Body) });
        }
    }
    [STAThread]
    private static int Main()
    {
        string directory = Path.Combine(Path.GetTempPath(), "SiusClientTests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            using (var form = new Form1())
            {
                SynchronizationContext.SetSynchronizationContext(null);
                object[] parse = { Shot(1), null };
                Check((bool)Call(form, "TryParseSiusShotRecord", parse), "valid shot");
                Check(((Form1.SiusShotRecord)parse[1]).Primaerwertung == 10.5m, "decimal comma");
                parse = new object[] { Shot(1).Replace("10,5", "1,2,3"), null };
                Check(!(bool)Call(form, "TryParseSiusShotRecord", parse), "reject malformed decimal");
                Check((string)Call(null, "CsvEscape", "Muster, \"Max\"") == "\"Muster, \"\"Max\"\"\"", "CSV quoting");
                Check(((string)Call(null, "ShooterCsv", new Form1.ShooterDto { Id = 1, Name = "Muster", Vorname = null }))
                    .StartsWith("1,Muster,,Muster,"), "empty first name");

                string import = Path.Combine(directory, "shots.csv");
                File.WriteAllText(import, Shot(1) + "\r\n" + Shot(2), Encoding.GetEncoding(28591));
                var block = Call(null, "ImportBlockLesen", import, 0L);
                Check(Get<System.Collections.Generic.List<string>>(block, "Zeilen").Count == 1, "defer incomplete final line");
                Check(Get<long>(block, "Position") == Shot(1).Length + 2, "byte cursor");
                File.AppendAllText(import, "\n", Encoding.GetEncoding(28591));
                block = Call(null, "ImportBlockLesen", import, Get<long>(block, "Position"));
                Check(Get<System.Collections.Generic.List<string>>(block, "Zeilen").Single() == Shot(2), "finish partial line");
                block = Call(null, "ImportBlockLesen", import, 999999L);
                Check(Get<System.Collections.Generic.List<string>>(block, "Zeilen").Count == 2, "truncation restart");

                var handler = new Handler { Status = HttpStatusCode.InternalServerError };
                Get<HttpClient>(form, "_httpClient").Dispose();
                using (var client = new HttpClient(handler))
                {
                    Set(form, "_httpClient", client);
                    Set(form, "_serverAdresse", "http://localhost");
                    Set(form, "_importPfad", import);
                    Set(form, "_gewaehlterAnlass", new Form1.Anlass { Id = 1 });
                    File.WriteAllText(import, Shot(1) + "\n" + Shot(1) + "\n");
                    try
                    {
                        ((Task)Call(form, "ImportDateiPruefenUndSendenAsync", CancellationToken.None)).GetAwaiter().GetResult();
                        throw new Exception("HTTP failure was swallowed");
                    }
                    catch (HttpRequestException) { checks++; }
                    Check(Get<long>(form, "_importPosition") == 0, "failed send preserves cursor");
                    Check(Get<int>(form, "_anzahlGeleseneSchussdaten") == 0, "failed send preserves count");
                    handler.Status = HttpStatusCode.OK;
                    handler.Body = "{\"success\":false}";
                    try { ((Task)Call(form, "ImportDateiPruefenUndSendenAsync", CancellationToken.None)).GetAwaiter().GetResult(); }
                    catch (InvalidDataException) { checks++; }
                    Check(Get<long>(form, "_importPosition") == 0, "unconfirmed response preserves cursor");
                    handler.Body = "{\"success\":true,\"imported\":1}";
                    ((Task)Call(form, "ImportDateiPruefenUndSendenAsync", CancellationToken.None)).GetAwaiter().GetResult();
                    Check(Get<int>(form, "_anzahlGeleseneSchussdaten") == 1, "retry and batch deduplication");
                    Check(Get<long>(form, "_importPosition") == new FileInfo(import).Length, "successful send commits cursor");
                    ((Task)Call(form, "ImportDateiPruefenUndSendenAsync", CancellationToken.None)).GetAwaiter().GetResult();
                    Check(handler.Requests == 3, "committed rows are not resent");
                    File.AppendAllText(import, "invalid\n");
                    long position = Get<long>(form, "_importPosition");
                    try { ((Task)Call(form, "ImportDateiPruefenUndSendenAsync", CancellationToken.None)).GetAwaiter().GetResult(); }
                    catch (InvalidDataException) { checks++; }
                    Check(Get<long>(form, "_importPosition") == position, "invalid rows are not silently consumed");
                }

                string export = Path.Combine(directory, "shooters.csv");
                var rows = new System.Collections.Generic.List<string> { "1,Müller" };
                ((Task)Call(null, "DateiSchreibenAsync", export, rows, false, CancellationToken.None)).GetAwaiter().GetResult();
                ((Task)Call(null, "DateiSchreibenAsync", export, rows, true, CancellationToken.None)).GetAwaiter().GetResult();
                Check(File.ReadAllLines(export, Encoding.GetEncoding(28591)).Length == 2, "atomic append");
                byte[] original = File.ReadAllBytes(export);
                rows.Add("2,\u20ac");
                try { ((Task)Call(null, "DateiSchreibenAsync", export, rows, false, CancellationToken.None)).GetAwaiter().GetResult(); }
                catch (EncoderFallbackException) { checks++; }
                Check(original.SequenceEqual(File.ReadAllBytes(export)), "failed export preserves original");
                rows.RemoveAt(1);
                using (var locked = new FileStream(export, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    try { ((Task)Call(null, "DateiSchreibenAsync", export, rows, true, CancellationToken.None)).GetAwaiter().GetResult(); }
                    catch (IOException) { checks++; }
                }
                Check(original.SequenceEqual(File.ReadAllBytes(export)), "locked export preserves original");
                using (var cts = new CancellationTokenSource())
                {
                    cts.Cancel();
                    try { ((Task)Call(null, "DateiSchreibenAsync", export, rows, false, cts.Token)).GetAwaiter().GetResult(); }
                    catch (OperationCanceledException) { checks++; }
                }
                Check(original.SequenceEqual(File.ReadAllBytes(export)), "cancelled export preserves original");
                File.WriteAllLines(import, Enumerable.Range(1, 600).Select(i => Shot(i)));
                block = Call(null, "ImportBlockLesen", import, 0L);
                Check(Get<System.Collections.Generic.List<string>>(block, "Zeilen").Count == 500, "bounded batch size");
                Check(Directory.GetFiles(directory, "*.tmp").Length == 0, "temporary files cleaned");
            }
            Console.WriteLine(checks + " checks passed.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { Directory.Delete(directory, true); }
    }
}
