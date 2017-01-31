﻿//-------------------------------------------------------------------------------------
//     Copyright (c) 2014, Anthilla S.r.l. (http://www.anthilla.com)
//     All rights reserved.
//
//     Redistribution and use in source and binary forms, with or without
//     modification, are permitted provided that the following conditions are met:
//         * Redistributions of source code must retain the above copyright
//           notice, this list of conditions and the following disclaimer.
//         * Redistributions in binary form must reproduce the above copyright
//           notice, this list of conditions and the following disclaimer in the
//           documentation and/or other materials provided with the distribution.
//         * Neither the name of the Anthilla S.r.l. nor the
//           names of its contributors may be used to endorse or promote products
//           derived from this software without specific prior written permission.
//
//     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//     ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//     DISCLAIMED. IN NO EVENT SHALL ANTHILLA S.R.L. BE LIABLE FOR ANY
//     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//     20141110
//-------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using antdlib.common;
using antdlib.common.Helpers;
using antdlib.config;
using antdlib.views;
using Antd.Apps;
using Antd.Asset;
using Antd.Database;
using Antd.Overlay;
using Antd.Storage;
using Antd.SystemdTimer;
using Antd.Timer;
using Antd.Ui;
using Antd.Users;
using Nancy;
using Nancy.Hosting.Self;
using RaptorDB;
using HostConfiguration = antdlib.config.HostConfiguration;

namespace Antd {
    internal class Application {
        public static RaptorDB.RaptorDB Database;

        #region [    private classes init    ]
        private static readonly AclConfiguration AclConfiguration = new AclConfiguration();
        private static readonly ApplicationRepository ApplicationRepository = new ApplicationRepository();
        private static readonly AppTarget AppTarget = new AppTarget();
        private static readonly Bash Bash = new Bash();
        private static readonly BindConfiguration BindConfiguration = new BindConfiguration();
        private static readonly CaConfiguration CaConfiguration = new CaConfiguration();
        private static readonly DhcpdConfiguration DhcpdConfiguration = new DhcpdConfiguration();
        private static readonly FirewallConfiguration FirewallConfiguration = new FirewallConfiguration();
        private static readonly GlusterConfiguration GlusterConfiguration = new GlusterConfiguration();
        private static readonly HostConfiguration HostConfiguration = new HostConfiguration();
        private static readonly MountManagement Mount = new MountManagement();
        private static readonly NetworkConfiguration NetworkConfiguration = new NetworkConfiguration();
        private static readonly RsyncConfiguration RsyncConfiguration = new RsyncConfiguration();
        private static readonly SambaConfiguration SambaConfiguration = new SambaConfiguration();
        private static readonly SetupConfiguration SetupConfiguration = new SetupConfiguration();
        private static readonly SshdConfiguration SshdConfiguration = new SshdConfiguration();
        private static readonly SyslogNgConfiguration SyslogNgConfiguration = new SyslogNgConfiguration();
        private static readonly Timers Timers = new Timers();
        private static readonly UserConfiguration UserConfiguration = new UserConfiguration();
        private static readonly Zpool Zpool = new Zpool();
        private static readonly JournaldConfiguration JournaldConfiguration = new JournaldConfiguration();
        private static readonly SyncMachineConfiguration SyncMachineConfiguration = new SyncMachineConfiguration();
        #endregion

        private static void Main() {
            ConsoleLogger.Log("");
            ConsoleLogger.Log("starting antd");
            var startTime = DateTime.Now;
            Procedure();
            var app = new AppConfiguration().Get();
            var port = app.AntdPort;
            var uri = $"http://localhost:{app.AntdPort}/";
            var host = new NancyHost(new Uri(uri));
            host.Start();
            ConsoleLogger.Log("host ready");
            StaticConfiguration.DisableErrorTraces = false;
            ConsoleLogger.Log($"http port: {port}");
            ConsoleLogger.Log("antd is running");
            ConsoleLogger.Log($"loaded in: {DateTime.Now - startTime}");
            if(Environment.OSVersion.Platform == PlatformID.Unix) {
                KeepAlive();
                ConsoleLogger.Log("antd is closing");
                host.Stop();
                Console.WriteLine("host shutdown");
                Database.Shutdown();
                Console.WriteLine("database shutdown");
            }
            else {
                HandlerRoutine hr = ConsoleCtrlCheck;
                GC.KeepAlive(hr);
                SetConsoleCtrlHandler(hr, true);
                while(!_isclosing) { }
                Console.WriteLine("antd is stopping");
                host.Stop();
                Console.WriteLine("host shutdown");
                Database.Shutdown();
                Console.WriteLine("database shutdown");
            }
        }

        private static void Procedure() {
            var appConfiguration = new AppConfiguration().Get();

            #region [    Remove Limits    ]
            if(Parameter.IsUnix) {
                const string limitsFile = "/etc/security/limits.conf";
                if(File.Exists(limitsFile)) {
                    if(!File.ReadAllText(limitsFile).Contains("root - nofile 1024000")) {
                        File.AppendAllLines(limitsFile, new[] { "root - nofile 1024000" });
                    }
                }
                Bash.Execute("ulimit -n 1024000", false);
            }
            #endregion

            #region [    Overlay Watcher    ]
            new OverlayWatcher().StartWatching();
            ConsoleLogger.Log("overlay watcher ready");
            #endregion

            #region [    Check OS    ]
            Bash.Execute($"{Parameter.Aossvc} reporemountrw", false);
            ConsoleLogger.Log("os checked");
            #endregion

            #region [    Working Directories    ]
            Directory.CreateDirectory("/cfg/antd");
            Directory.CreateDirectory("/cfg/antd/database");
            Directory.CreateDirectory("/cfg/antd/services");
            Directory.CreateDirectory("/mnt/cdrom/DIRS");
            if(Parameter.IsUnix) {
                Mount.WorkingDirectories();
            }
            ConsoleLogger.Log("working directories ready");
            #endregion

            #region [    Database    ]
            Database = RaptorDB.RaptorDB.Open(appConfiguration.DatabasePath);
            Global.RequirePrimaryView = false;
            Global.BackupCronSchedule = null;
            Global.SaveIndexToDiskTimerSeconds = 30;
            Database.RegisterView(new ApplicationView());
            Database.RegisterView(new AuthorizedKeysView());
            Database.RegisterView(new TimerView());
            ConsoleLogger.Log("database ready");
            #endregion

            if(Parameter.IsUnix) {
                #region [    Mounts    ]
                if(MountHelper.IsAlreadyMounted("/mnt/cdrom/Kernel/active-firmware", "/lib64/firmware") == false) {
                    Bash.Execute("mount /mnt/cdrom/Kernel/active-firmware /lib64/firmware", false);
                }
                var kernelRelease = Bash.Execute("uname -r").Trim();
                var linkedRelease = Bash.Execute("file /mnt/cdrom/Kernel/active-modules").Trim();
                if(MountHelper.IsAlreadyMounted("/mnt/cdrom/Kernel/active-modules") == false &&
                    linkedRelease.Contains(kernelRelease)) {
                    var moduleDir = $"/lib64/modules/{kernelRelease}/";
                    Directory.CreateDirectory(moduleDir);
                    Bash.Execute($"mount /mnt/cdrom/Kernel/active-modules {moduleDir}", false);
                }
                Bash.Execute("systemctl restart systemd-modules-load.service", false);
                Mount.AllDirectories();
                ConsoleLogger.Log("mounts ready");
                #endregion

                #region [    JournalD    ]
                if(JournaldConfiguration.IsActive()) {
                    JournaldConfiguration.Set();
                }
                #endregion

                #region [    MachineID    ]
                ConsoleLogger.Log($"[machineid] {Machine.MachineId.Get}");
                #endregion

                #region [    Host Configuration    ]
                HostConfiguration.ApplyHostInfo();
                ConsoleLogger.Log("host configured");
                #endregion

                #region [    Name Service    ]
                HostConfiguration.ApplyNsHosts();
                HostConfiguration.ApplyNsNetworks();
                HostConfiguration.ApplyNsResolv();
                HostConfiguration.ApplyNsSwitch();
                ConsoleLogger.Log("name service ready");
                #endregion

                #region [    OS Parameters    ]
                HostConfiguration.ApplyHostOsParameters();
                ConsoleLogger.Log("os parameters ready");
                #endregion

                #region [    Modules    ]
                HostConfiguration.ApplyHostBlacklistModules();
                HostConfiguration.ApplyHostModprobes();
                HostConfiguration.ApplyHostRemoveModules();
                ConsoleLogger.Log("modules ready");
                #endregion

                #region [    Import Commands    ]
                File.Copy($"{Parameter.RootFrameworkAntdShellScript}/var/kerbynet.conf", "/etc/kerbynet.conf", true);
                ConsoleLogger.Log("commands and scripts configuration imported");
                #endregion
            }

            #region [    Users    ]
            var manageMaster = new ManageMaster();
            manageMaster.Setup();
            if(Parameter.IsUnix) {
                UserConfiguration.Import();
                UserConfiguration.Set();
            }
            ConsoleLogger.Log("users config ready");
            #endregion

            if(!Parameter.IsUnix)
                return;

            #region [    Time & Date    ]
            HostConfiguration.ApplyNtpdate();
            HostConfiguration.ApplyTimezone();
            HostConfiguration.ApplyNtpd();
            ConsoleLogger.Log("time and date configured");
            #endregion

            #region [    Network    ]
            NetworkConfiguration.Start();
            NetworkConfiguration.ApplyDefaultInterfaceSetting();
            #endregion

            #region [    Apply Setup Configuration    ]
            SetupConfiguration.Set();
            ConsoleLogger.Log("machine configured");
            #endregion

            #region [    Services    ]
            HostConfiguration.ApplyHostServices();
            ConsoleLogger.Log("services ready");
            #endregion

            #region [    Ssh    ]
            if(SshdConfiguration.IsActive()) {
                SshdConfiguration.Set();
            }
            if(!Directory.Exists(Parameter.RootSsh)) {
                Directory.CreateDirectory(Parameter.RootSsh);
            }
            if(!Directory.Exists(Parameter.RootSshMntCdrom)) {
                Directory.CreateDirectory(Parameter.RootSshMntCdrom);
            }
            if(!MountHelper.IsAlreadyMounted(Parameter.RootSsh)) {
                var mnt = new MountManagement();
                mnt.Dir(Parameter.RootSsh);
            }
            var rk = new RootKeys();
            if(rk.Exists == false) {
                rk.Create();
            }
            var storedKeyRepo = new AuthorizedKeysRepository();
            var storedKeys = storedKeyRepo.GetAll();
            foreach(var storedKey in storedKeys) {
                var home = storedKey.User == "root" ? "/root/.ssh" : $"/home/{storedKey.User}/.ssh";
                var authorizedKeysPath = $"{home}/authorized_keys";
                if(!File.Exists(authorizedKeysPath)) {
                    File.Create(authorizedKeysPath);
                }
                File.AppendAllLines(authorizedKeysPath, new List<string> { $"{storedKey.KeyValue} {storedKey.RemoteUser}" });
                Bash.Execute($"chmod 600 {authorizedKeysPath}");
                Bash.Execute($"chown {storedKey.User}:{storedKey.User} {authorizedKeysPath}");
            }
            ConsoleLogger.Log("ssh ready");
            #endregion

            #region [    Firewall    ]
            if(FirewallConfiguration.IsActive()) {
                FirewallConfiguration.Set();
            }
            #endregion

            #region [    Dhcpd    ]
            if(DhcpdConfiguration.IsActive()) {
                DhcpdConfiguration.Set();
            }
            #endregion

            #region [    Bind    ]
            if(BindConfiguration.IsActive()) {
                BindConfiguration.Set();
            }
            #endregion

            #region [    Samba    ]
            if(SambaConfiguration.IsActive()) {
                SambaConfiguration.Set();
            }
            #endregion

            #region [    Syslog    ]
            if(SyslogNgConfiguration.IsActive()) {
                SyslogNgConfiguration.Set();
            }
            #endregion

            #region [    Avahi    ]
            const string avahiServicePath = "/etc/avahi/services/antd.service";
            if(File.Exists(avahiServicePath)) {
                File.Delete(avahiServicePath);
            }
            File.WriteAllLines(avahiServicePath, AvahiCustomXml.Generate(appConfiguration.AntdPort.ToString()));
            Bash.Execute("chmod 755 /etc/avahi/services", false);
            Bash.Execute($"chmod 644 {avahiServicePath}", false);
            Systemctl.Restart("avahi-daemon.service");
            Systemctl.Restart("avahi-daemon.socket");
            ConsoleLogger.Log("avahi ready");
            #endregion

            #region [    Storage    ]
            foreach(var pool in Zpool.ImportList().ToList()) {
                if(string.IsNullOrEmpty(pool))
                    continue;
                ConsoleLogger.Log($"pool {pool} imported");
                Zpool.Import(pool);
            }
            ConsoleLogger.Log("storage ready");
            #endregion

            #region [    Scheduler    ]
            Timers.CleanUp();
            Timers.Setup();
            Timers.Import();
            Timers.Export();
            foreach(var zp in Zpool.List()) {
                Timers.Create(zp.Name.ToLower() + "snap", "hourly", $"/sbin/zfs snap -r {zp.Name}@${{TTDATE}}");
            }
            Timers.StartAll();
            new SnapshotCleanup().Start(new TimeSpan(2, 00, 00));
            new SyncTime().Start(new TimeSpan(0, 42, 00));
            new RemoveUnusedModules().Start(new TimeSpan(2, 15, 00));
            ConsoleLogger.Log("scheduled events ready");
            #endregion

            #region [    Acl    ]
            if(AclConfiguration.IsActive()) {
                AclConfiguration.Set();
                AclConfiguration.ScriptSetup();
            }
            #endregion

            #region [    Sync    ]
            if(GlusterConfiguration.IsActive()) {
                GlusterConfiguration.Set();
            }
            if(RsyncConfiguration.IsActive()) {
                RsyncConfiguration.Set();
            }
            #endregion

            #region [    SyncMachine    ]
            if(SyncMachineConfiguration.IsActive()) {
                SyncMachineConfiguration.Set();
            }
            #endregion

            #region [    C A    ]
            if(CaConfiguration.IsActive()) {
                CaConfiguration.Set();
            }
            #endregion

            #region [    Apps    ]
            AppTarget.Setup();
            var apps = ApplicationRepository.GetAll().Select(_ => new ApplicationModel(_)).ToList();
            foreach(var app in apps) {
                var units = app.UnitLauncher;
                foreach(var unit in units) {
                    if(Systemctl.IsActive(unit) == false) {
                        Systemctl.Restart(unit);
                    }
                }
            }
            //AppTarget.StartAll();
            ConsoleLogger.Log("apps ready");
            #endregion

            #region [    AntdUI    ]
            UiService.Setup();
            ConsoleLogger.Log("antduisetup");
            #endregion
        }

        #region [    Shutdown Management    ]
        private static void KeepAlive() {
            var r = Console.ReadLine();
            while(r != "quit") {
                r = Console.ReadLine();
            }
        }

        private static bool _isclosing;

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType) {
            Database.Shutdown();
            switch(ctrlType) {
                case CtrlTypes.CtrlCEvent:
                    _isclosing = true;
                    Console.WriteLine("CTRL+C received!");
                    break;
                case CtrlTypes.CtrlBreakEvent:
                    _isclosing = true;
                    Console.WriteLine("CTRL+BREAK received!");
                    break;
                case CtrlTypes.CtrlCloseEvent:
                    _isclosing = true;
                    Console.WriteLine("Program being closed!");
                    break;
                case CtrlTypes.CtrlLogoffEvent:
                case CtrlTypes.CtrlShutdownEvent:
                    _isclosing = true;
                    Console.WriteLine("User is logging off!");
                    break;
            }
            return true;
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

        public delegate bool HandlerRoutine(CtrlTypes ctrlType);

        public enum CtrlTypes {
            CtrlCEvent = 0,
            CtrlBreakEvent,
            CtrlCloseEvent,
            CtrlLogoffEvent = 5,
            CtrlShutdownEvent
        }
        #endregion
    }
}