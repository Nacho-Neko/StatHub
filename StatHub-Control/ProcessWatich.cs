using FramRpcService;
using Spectre.Console;
using StatHub.Control.Model;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace StatHub.Control
{
    internal class ProcessWatich
    {
        private bool flag = true;
        private Process process;
        private string arguments;

        private Table StatusTable;
        private Table IncomeTable;
        private Layout layout;

        private readonly List<DiskConsole> StatusConsoles = new List<DiskConsole>();
        private readonly List<IncomeConsole> IncomeConsoles = new List<IncomeConsole>();


        private static string filePath = "/tmp/farm";
        private readonly string logDirectory;
        private readonly string logFilePath;
        private readonly StreamWriter streamWriter;
        public static string uuid;

        private int _index;
        public ProcessWatich(string logDirectory)
        {
            ConsoleInit();

            this.logDirectory = logDirectory;
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH-mm-ss-fff");
            string logFileName = $"log_{timestamp}.txt";
            logFilePath = Path.Combine(logDirectory, logFileName);
            process = new Process();
            streamWriter = new StreamWriter(logFilePath, append: true);
            streamWriter.AutoFlush = true;
        }
        public static async Task<string?> Init()
        {
            string? md5Hash = null;
            if (File.Exists(filePath))
            {
                Console.WriteLine("文件存在。计算MD5...");
                md5Hash = GetMd5Hash(filePath);
                Console.WriteLine($"文件MD5: {md5Hash}");
            }
            else
            {
                Console.WriteLine("文件不存在。");
            }
            uuid = Guid.NewGuid().ToString();
            return md5Hash;
        }
        static string GetMd5Hash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hashBytes = md5.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }
        }
        private async void ConsoleInit()
        {
            // 创建一个表格
            StatusTable = new Table();
            // 添加列
            StatusTable.AddColumn("硬盘序号");
            StatusTable.AddColumn("路径");
            StatusTable.AddColumn("大小");
            StatusTable.AddColumn("状态");
            StatusTable.AddColumn("进度");
            StatusTable.AddColumn("出块间隔");
            StatusTable.AddColumn("状态更新");
            // 设置表格样式
            StatusTable.Border(TableBorder.Rounded);


            IncomeTable = new Table();
            IncomeTable.AddColumn("硬盘序号");
            IncomeTable.AddColumn("日收入");
            IncomeTable.AddColumn("总收入");
            IncomeTable.Border(TableBorder.Rounded);

            AnsiConsole.Clear();
            // AnsiConsole.Write(layout);

            // 在控制台中显示表格
            // AnsiConsole.Write(table);
            await Task.Run(ConsoleUpdate);
        }

        private async Task ConsoleUpdate()
        {
            Thread.Sleep(1000);
            // logQueue.Log("测试");
            while (flag)
            {
                if (StatusConsoles.Count > 0)
                {
                    StatusTable.Rows.Clear();
                    for (int i = 0; i < StatusConsoles.Count; i++)
                    {
                        DiskConsole diskConsole = StatusConsoles[i];
                        TimeSpan timeDifference = DateTime.Now - diskConsole.update_at;
                        double seconds = timeDifference.TotalSeconds;
                        StatusTable.AddRow(i.ToString(), diskConsole.path, diskConsole.size, diskConsole.status, diskConsole.progress, diskConsole.ToIntervar(diskConsole.interval), diskConsole.ToIntervar(seconds));
                    }
                }

                if (IncomeConsoles.Count > 0)
                {
                    IncomeTable.Rows.Clear();
                    for (int i = 0; i < IncomeConsoles.Count; i++)
                    {
                        IncomeConsole incomeConsole = IncomeConsoles[i];
                        IncomeTable.AddRow(i.ToString(), incomeConsole.IncomeDay.ToString("F1") + " tSSC", incomeConsole.IncomeTotal.ToString("F1") + " tSSC");
                    }
                }

                AnsiConsole.Clear();
                try
                {
                    AnsiConsole.Write(StatusTable);
                    AnsiConsole.Write(IncomeTable);
                }
                catch (Exception ex)
                {
                    streamWriter.WriteLine(ex);
                }
                Thread.Sleep(300);
            }
        }


        public async Task StartAsync(string arguments)
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = arguments,
                RedirectStandardInput = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            process.OutputDataReceived += Analysis;
            process.ErrorDataReceived += Error;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            Console.WriteLine($"进程 {process.Id} 启动...");
            Console.WriteLine($"/dev/farm {arguments}");
            await process.WaitForExitAsync();
            flag = false;
            Console.WriteLine($"进程 {process.Id} 退出，正在重启...");
        }

        internal async void Analysis(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string log = e.Data;
                streamWriter.WriteLine(log);
                LogEntry logEntry = LogParser.LogParse(log);
                if (logEntry != null)
                {
                    // Console.WriteLine(logEntry.Message);
                    if (logEntry.Message.Contains("Plotting sector"))
                    {
                        Sector sector = LogParser.CompleteParse(logEntry);
                        if (int.TryParse(sector.DiskFarmIndex, out int index))
                        {
                            DiskConsole diskConsole = StatusConsoles[index];
                            progress(diskConsole, "Plotting", sector.CompletionPercentage);

                            DateTime now = DateTime.Now;
                            TimeSpan timeDifference = now - diskConsole.update_at;
                            diskConsole.interval = timeDifference.TotalSeconds;
                            diskConsole.update_at = now;
                        }
                        // Console.WriteLine($"disk:{DiskFarmIndex} 进度:{CompletionPercentage}");
                    }
                    else if (logEntry.Message.Contains("Replotting sector"))
                    {
                        Sector sector = LogParser.CompleteParse(logEntry);
                        if (int.TryParse(sector.DiskFarmIndex, out int index))
                        {
                            DiskConsole diskConsole = StatusConsoles[index];
                            progress(diskConsole, "Replotting", sector.CompletionPercentage);

                            DateTime now = DateTime.Now;
                            TimeSpan timeDifference = now - diskConsole.update_at;
                            diskConsole.interval = timeDifference.TotalSeconds;
                            diskConsole.update_at = now;
                        }
                    }
                    /// Piece cache sync 0.42% complete
                    else if (logEntry.Message.Contains("Piece cache sync"))
                    {
                        string complete = LogParser.disk_piece_sync(logEntry.Message);
                        progress("Piece Sync", complete);
                    }
                    else if (logEntry.Message.Contains("Successfully signed reward hash"))
                    {
                        // igned
                        int index = LogParser.disk_farm_index(logEntry.Source);
                        IncomeConsole incomeConsole = IncomeConsoles[index];

                        if (DateTime.Now.Date > incomeConsole.DateTime.Date)
                        {
                            incomeConsole.DateTime = DateTime.Now.Date;
                            incomeConsole.IncomeDay = 0;
                        }
                        incomeConsole.IncomeDay += 0.1;
                        incomeConsole.IncomeTotal += 0.1;

                        var serializer = new SerializerBuilder().Build();
                        string yaml = serializer.Serialize(IncomeConsoles);
                        File.WriteAllText("income.yaml", yaml);
                    }
                    else if (logEntry.Message.Contains("Initial plotting complete"))
                    {
                        int index = LogParser.disk_farm_index(logEntry.Source);
                        DiskConsole diskConsole = StatusConsoles[index];
                        progress(diskConsole, "Plotting Complete", "100%");
                    }
                    else if (logEntry.Message.Contains("Replotting complete"))
                    {
                        int index = LogParser.disk_farm_index(logEntry.Source);
                        DiskConsole diskConsole = StatusConsoles[index];
                        progress(diskConsole, "Replotting Complete", "100%");
                    }
                    else if (logEntry.Message.Contains("Initializing piece cache"))
                    {
                        progress("Initializing Piece", "0%");
                        //Console.WriteLine($"Piece Init");
                    }
                    else if (logEntry.Message.Contains("Synchronizing piece cache"))
                    {
                        progress("Synchronizing Piece", "0%");
                        //Console.WriteLine(log);
                    }
                    else if (logEntry.Message.Contains("Finished piece cache synchronization"))
                    {
                        progress("Finished", "100%");
                        //Console.WriteLine($"Piece Synced");
                    }
                    return;
                }
                else
                {
                    if (log.Contains("Failed to retrieve piece from node right after archiving, this should never happen and is an implementation bug error=Request timeout segment_index"))
                    {
                        process.Close();
                        Console.WriteLine(log);
                        Environment.Exit(0);
                    }
                    if (log.Contains("Piece getter must returns valid pieces of history that contain proper scalar bytes; qed:"))
                    {
                        Console.WriteLine(log);
                        Environment.Exit(0);
                    }
                    // Star Info 匹配
                    string[] str = log.Split(":");
                    if (str[0].Contains("Single disk farm"))
                    {
                        string pattern = @"farm\s+(\d+)";
                        Match match = Regex.Match(str[0], pattern);
                        if (match.Success)
                        {
                            int.TryParse(match.Groups[1].Value, out _index);
                            StatusConsoles.Add(new DiskConsole("", "", "0%"));
                            IncomeConsoles.Add(new IncomeConsole(_index));
                            // Console.WriteLine($"添加硬盘 : {_index}");
                        }
                    }
                    else if (str[0].Contains("Allocated space"))
                    {
                        DiskConsole diskConsole = StatusConsoles[_index];
                        diskConsole.size = str[1];
                        // Console.WriteLine($"    空间 : {str[1]}");
                    }
                    else if (str[0].Contains("Directory"))
                    {
                        DiskConsole diskConsole = StatusConsoles[_index];
                        IncomeConsole incomeConsole = IncomeConsoles[_index];
                        diskConsole.path = str[1];
                        incomeConsole.DateTime = DateTime.Now.Date;
                        incomeConsole.Path = str[1];
                        string yaml;
                        if (File.Exists("income.yaml"))
                        {
                            yaml = File.ReadAllText("income.yaml");
                            var yamlDeserializer = new DeserializerBuilder().Build();
                            var config = yamlDeserializer.Deserialize<IncomeSetting[]>(yaml);
                            IncomeSetting? incomeSetting = config.Where(it => it.Id == _index).FirstOrDefault();
                            if (incomeSetting != null)
                            {
                                if (DateTime.Now.Date > incomeSetting.DateTime.Date)
                                {
                                    incomeSetting.IncomeDay = 0;
                                }
                                else
                                {
                                    incomeConsole.IncomeDay = incomeSetting.IncomeDay;
                                }
                                incomeConsole.DateTime = incomeSetting.DateTime;
                                incomeConsole.IncomeTotal = incomeSetting.IncomeTotal;
                            }
                        }
                        var serializer = new SerializerBuilder().Build();
                        yaml = serializer.Serialize(IncomeConsoles);
                        File.WriteAllText("income.yaml", yaml);
                        // Console.WriteLine($"    路径 : {str[1]}");
                    }
                }
            }
        }

        public void progress(DiskConsole diskConsole, string status, string progress)
        {
            diskConsole.progress = progress;
            diskConsole.status = status;
        }

        public void progress(string status, string progress)
        {
            for (int i = 0; i < StatusConsoles.Count; i++)
            {
                DiskConsole diskConsole = StatusConsoles[i];
                diskConsole.progress = progress;
                diskConsole.status = status;
            }
        }
        internal async void Error(object sender, DataReceivedEventArgs e)
        {
            streamWriter.WriteLine(e.Data);
            Console.WriteLine(e.Data);
        }
        public async Task<string> ConstrucAsync(FarmRpc.FarmRpcClient farmRpc)
        {
            try
            {
                if (farmRpc != null)
                {
                    var request = new StarArgRequest { Mainet = false };
                    var reply = await farmRpc.StarArgAsync(request);
                    var ArgBuildewr = new List<string> {
                        "farm",
                        $"--reward-address {reply.RewardAddress}",
                        $"--node-rpc-url {reply.NodeRpcUrl}"
                    };
                    if (reply.ReservedPeers != null && reply.ReservedPeers.Count > 0)
                        foreach (var reserved_peer in reply.ReservedPeers)
                            ArgBuildewr.Add($"--reserved-peers {reserved_peer}");
                    if (reply.AllowPrivateIps.HasValue && reply.AllowPrivateIps == true)
                        ArgBuildewr.Add("--allow-private-ips");
                    if (reply.DisableBootstrapOnStart.HasValue && reply.DisableBootstrapOnStart == true)
                        ArgBuildewr.Add($"--disable-bootstrap-on-start");
                    if (reply.FarmDuringInitialPlotting.HasValue)
                        ArgBuildewr.Add($"--farm-during-initial-plotting {reply.FarmDuringInitialPlotting.ToString().ToLower()}");
                    foreach (var path in reply.Path)
                    {
                        ArgBuildewr.Add(path);
                    }
                    arguments = string.Join(" ", ArgBuildewr);
                    return arguments;
                }
                return arguments;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // 服务器没有返回或出现问题 使用上次返回
                return arguments;
            }
        }
        public async Task<string> ConstrucAsync(FarmSetting appSettings)
        {
            try
            {
                if (appSettings != null)
                {
                    var ArgBuildewr = new List<string> {
                        "farm",
                        $"--reward-address {appSettings.reward_address}"
                    };
                    if (!string.IsNullOrEmpty(appSettings.node_rpc_url))
                        ArgBuildewr.Add($"--node-rpc-url {appSettings.node_rpc_url}");
                    else
                        ArgBuildewr.Add($"--node-rpc-url ws://node-0.gemini-3h.subspace.network.ooplay.cn:80");

                    if (appSettings.reserved_peers != null && appSettings.reserved_peers.Count > 0)
                        foreach (var reserved_peer in appSettings.reserved_peers)
                            ArgBuildewr.Add($"--reserved-peers {reserved_peer}");
                    if (appSettings.allow_private_ips.HasValue && appSettings.allow_private_ips == true)
                        ArgBuildewr.Add("--allow-private-ips");
                    if (appSettings.disable_bootstrap_on_start.HasValue && appSettings.disable_bootstrap_on_start == true)
                        ArgBuildewr.Add($"--disable-bootstrap-on-start");

                    if (appSettings.plotting_thread.HasValue)
                        ArgBuildewr.Add($"--plotting-thread-pool-size {appSettings.plotting_thread}");
                    if (appSettings.replotting_thread.HasValue)
                        ArgBuildewr.Add($"--replotting-thread-pool-size {appSettings.replotting_thread}");
                    if (appSettings.farming_thread.HasValue)
                        ArgBuildewr.Add($"--farming-thread-pool-size {appSettings.farming_thread}");

                    if (appSettings.farm_during_initial_plotting.HasValue)
                        ArgBuildewr.Add($"--farm-during-initial-plotting {appSettings.farm_during_initial_plotting.ToString().ToLower()}");
                    foreach (string path in appSettings.paths)
                    {
                        ArgBuildewr.Add($"path={path}");
                    }

                    if (appSettings.listen != null)
                        foreach (string listen in appSettings.listen)
                        {
                            ArgBuildewr.Add($"--listen-on={listen}");
                        }

                    arguments = string.Join(" ", ArgBuildewr);
                    return arguments;
                }
                return arguments;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // 服务器没有返回或出现问题 使用上次返回
                return arguments;
            }
        }
        internal void CheckUpdate(string? md5Hash)
        {

        }
    }
}
