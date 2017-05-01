using Nett;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace BatchExecService
{
    public partial class BatchExecService : ServiceBase
    {
        public BatchExecService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // derive the configuration file path
            var exeFilePath = Assembly.GetEntryAssembly().Location;
            var exeDirFilePath = Path.GetDirectoryName(exeFilePath);
            var exeFileStem = Path.GetFileNameWithoutExtension(exeFilePath);
            var configFilePath = Path.Combine(exeDirFilePath, exeFileStem + ".toml");

            var config = Toml.ReadFile<Config>(configFilePath);

            processesMtx.WaitOne();

            processes = config.CmdGroups
                .Select(cmdGroup =>
                {
                    var startInfo = new ProcessStartInfo
                    {
                        WindowStyle = cmdGroup.WindowStyle,
                        FileName = cmdGroup.FileName,
                        Arguments = cmdGroup.Arguments,
                    };

                    var process = new Process
                    {
                        StartInfo = startInfo,
                    };

                    process.Start();
                    return process;
                })
                .ToArray();

            processesMtx.ReleaseMutex();

            // asynchronously wait for all exit codes
            //exitCodes = WaitAll();
        }

        protected override void OnStop()
        {
            processesMtx.WaitOne();

            if (processes != null)
            {
                foreach (var process in processes)
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }
            }

            processesMtx.ReleaseMutex();
        }

        //private Task<IEnumerable<int>> WaitAll()
        //{
        //    return Task.Run<IEnumerable<int>>(() =>
        //    {
        //        processesMtx.WaitOne();

        //        var exitCodes = processes
        //            .Select(process =>
        //            {
        //                process.WaitForExit();
        //                return process.ExitCode;
        //            })
        //            .ToArray();

        //        processesMtx.ReleaseMutex();

        //        Stop();
        //        return exitCodes;
        //    });
        //}

        private Mutex processesMtx = new Mutex();
        private IEnumerable<Process> processes = null;
        //private Task<IEnumerable<int>> exitCodes = null;
    }
}
