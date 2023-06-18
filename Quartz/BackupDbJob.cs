using FilesAPI.Services;
using Quartz;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace FilesAPI.Quartz
{
    public class BackupDbJob : IJob
    {
        readonly static string set = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "set " : "export ";
        public async Task Execute(IJobExecutionContext context)
        {
            var dt = DateTime.Now.ToString("dd_MM_yyyy");
            var fileName = $"dump_{dt}.sql";
            await PostgreSqlDump(
                                  fileName,
                                  "localhost",
                                  "5432",
                                  "filesdb",
                                  "postgres",
                                  "1"
            );
        }

        //public static async Task Test()
        //{
        //    var dt = DateTime.Now.ToString("dd_MM_yyyy");
        //    var fileName = $"dump_{dt}.sql";
        //    await PostgreSqlDump(
        //                          fileName,
        //                          "localhost",
        //                          "5432",
        //                          "filesdb",
        //                          "postgres",
        //                          "1"
        //    );
        //}

        private static async Task PostgreSqlDump(
          string outFile,
          string host,
          string port,
          string database,
          string user,
          string password)
        {
            string dumpCommand =
                 $"{set}PGPASSWORD={password}\n" +
                 $"Q:\\postgresql\\bin\\pg_dump.exe" + " -Fc" + " -h " + host + " -p " + port + " -d " + database + " -U " + user + " -w";

            string batchContent = "" + dumpCommand + " > " + "\"" + outFile + "\"" + "\n";
            if (File.Exists(outFile)) File.Delete(outFile);

            await Execute(batchContent);
        }

        private static Task Execute(string dumpCommand)
        {
            return Task.Run(() =>
            {

                string batFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}." + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bat" : "sh"));
                try
                {
                    string batchContent = "";
                    batchContent += $"{dumpCommand}";

                    File.WriteAllText(batFilePath, batchContent, Encoding.ASCII);

                    ProcessStartInfo info = ProcessInfoByOS(batFilePath);

                    using Process proc = Process.Start(info);

                    proc.WaitForExit();
                    var exit = proc.ExitCode;

                    proc.Close();
                }
                catch (Exception e)
                {
                    throw;

                }
                finally
                {
                    if (File.Exists(batFilePath)) File.Delete(batFilePath);
                }
            });
        }

        private static ProcessStartInfo ProcessInfoByOS(string batFilePath)
        {
            ProcessStartInfo info;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                info = new ProcessStartInfo(batFilePath)
                {
                };
            }
            else
            {
                info = new ProcessStartInfo("sh")
                {
                    Arguments = $"{batFilePath}"
                };
            }

            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            info.RedirectStandardError = true;

            return info;
        }
    }
}
