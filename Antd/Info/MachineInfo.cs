﻿////-------------------------------------------------------------------------------------
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

using antdlib.common;
using antdlib.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Antd.Info {
    public class MachineInfo {

        private static string GetFileHash(string filePath) {
            using(var fileStreamToRead = File.OpenRead(filePath)) {
                return BitConverter.ToString(new SHA1Managed().ComputeHash(fileStreamToRead)).Replace("-", string.Empty);
            }
        }

        private static readonly MapToModel Mapper = new MapToModel();

        public IEnumerable<CpuinfoModel> GetCpuinfo() {
            var result = Mapper.FromFile<CpuinfoModel>("/proc/cpuinfo", ":");
            return result;
        }

        public IEnumerable<MeminfoModel> GetMeminfo() {
            var result = Mapper.FromFile<MeminfoModel>("/proc/meminfo", ":");
            return result;
        }

        public IEnumerable<AosReleaseModel> GetAosrelease() {
            var result = Mapper.FromFile<AosReleaseModel>("/etc/aos-release", ":");
            return result;
        }

        public IEnumerable<LosetupModel> GetLosetup() {
            var result = Mapper.FromCommand<LosetupModel>("losetup --list -n").ToList();
            return result;
        }

        public IEnumerable<UnitModel> GetServices() {
            var result = Mapper.FromCommand<UnitModel>("systemctl --no-pager list-unit-files").ToList().Skip(1);
            return result;
        }

        public List<UnitModel> GetUnits(string type) {
            var result = Mapper.FromCommand<UnitModel>($"systemctl list-units --no-legend --no-pager -t {type}", 3).ToList().Skip(1).ToList();
            foreach(var r in result) {
                r.Type = type;
            }
            return result;
        }

        public IEnumerable<ModuleModel> GetModules() {
            var result = Mapper.FromCommand<ModuleModel>("lsmod").ToList().Skip(1);
            return result;
        }

        private static readonly Bash Bash = new Bash();

        public UptimeModel GetUptime() {
            var result = Bash.Execute("uptime");
            var values = result.Split(new[] { "," }, 3, StringSplitOptions.RemoveEmptyEntries);
            var model = new UptimeModel {
                Uptime = values[0],
                Users = values[1],
                LoadAverage = values[2]
            };
            return model;
        }

        public IEnumerable<FreeModel> GetFree() {
            var result = Mapper.FromCommand<FreeModel>("free -lth").ToList().Skip(1);
            return result;
        }

        public IEnumerable<SystemComponentModel> GetSystemComponentModels() {
            var repoSystem = Parameter.RepoSystem;
            var actives = Directory.EnumerateFileSystemEntries(repoSystem).Where(_ => _.Contains("active-")).ToList();
            var repoKernel = Parameter.RepoKernel;
            actives.AddRange(Directory.EnumerateFileSystemEntries(repoKernel).Where(_ => _.Contains("active-")).ToList());
            var components = new List<SystemComponentModel>();
            var losetup = GetLosetup().ToList();
            foreach(var file in actives) {
                var alias = file.SplitToList("-").LastOrDefault();
                var dir = file.SplitToList("active-").LastOrDefault();
                var active = Bash.Execute($"file {file}").SplitToList("symbolic link to ").LastOrDefault();
                var recovery = Bash.Execute($"file {file.Replace("active", "recovery")}").SplitToList(":").LastOrDefault()?.Replace("symbolic link to", "");
                var hash = File.Exists(dir + "/" + active) ? GetFileHash(dir + "/" + active) : "";
                var running = losetup.Any(_ => _.Hash == hash && _.Backfile == dir + "/" + active) ? "1" : "0";
                var comp = new SystemComponentModel {
                    Alias = alias,
                    Active = active,
                    Recovery = recovery,
                    Running = running
                };
                components.Add(comp);
            }
            return components;
        }
    }
}
