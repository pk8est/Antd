﻿///-------------------------------------------------------------------------------------
///     Copyright (c) 2014, Anthilla S.r.l. (http://www.anthilla.com)
///     All rights reserved.
///
///     Redistribution and use in source and binary forms, with or without
///     modification, are permitted provided that the following conditions are met:
///         * Redistributions of source code must retain the above copyright
///           notice, this list of conditions and the following disclaimer.
///         * Redistributions in binary form must reproduce the above copyright
///           notice, this list of conditions and the following disclaimer in the
///           documentation and/or other materials provided with the distribution.
///         * Neither the name of the Anthilla S.r.l. nor the
///           names of its contributors may be used to endorse or promote products
///           derived from this software without specific prior written permission.
///
///     THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
///     ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
///     WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
///     DISCLAIMED. IN NO EVENT SHALL ANTHILLA S.R.L. BE LIABLE FOR ANY
///     DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
///     (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
///     LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
///     ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
///     (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
///     SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
///
///     20141110
///-------------------------------------------------------------------------------------

using System.IO;

namespace Antd.UnitFiles {
    public class UnitFile {
        public static void Write(string name) {
            UnitModel unitModel = UnitRepo.GetInfo(name);
            string folder = "/cfg/anthilla.units.d/";
            System.IO.Directory.CreateDirectory(folder);
            string file = unitModel.description;
            string path = Path.Combine(folder, file);
            if (!File.Exists(path)) {
                using (StreamWriter sw = File.CreateText(path)) {
                    sw.WriteLine("[Unit]");
                    sw.WriteLine("Description=" + unitModel.description);
                    sw.WriteLine("");
                    sw.WriteLine("[Service]");
                    sw.WriteLine("TimeoutStartSec=" + unitModel.timeOutStartSec);
                    sw.WriteLine("ExecStart=" + unitModel.execStart);
                    sw.WriteLine("");
                    sw.WriteLine("[Install]");
                    sw.WriteLine("Alias=" + unitModel.alias);
                    sw.WriteLine("");
                }
            }
            Command.Launch("chmod", "777 " + path);
        }

        public static void WriteForSelf() {
            string folder = "/cfg/anthilla.units.d/";
            Directory.CreateDirectory(folder);
            string file = "antd.service";
            string path = Path.Combine(folder, file);
            if (!File.Exists(path)) {
                using (StreamWriter sw = File.CreateText(path)) {
                    sw.WriteLine("[Unit]");
                    sw.WriteLine("Description=antd.service");
                    sw.WriteLine("");
                    sw.WriteLine("[Service]");
                    sw.WriteLine("TimeoutStartSec=0");
                    sw.WriteLine("ExecStartPre=mount /mnt/cdrom/DIR_usr_lib64_mono.squash.xz /usr/lib64/mono");
                    sw.WriteLine("ExecStartPre=mkdir /antd");
                    sw.WriteLine("ExecStartPre=mount mount /mnt/cdrom/DIR_antd_2015.squash.xz /antd");
                    sw.WriteLine("ExecStartPre=#mount -t tmpfs none /antd/cfg");
                    sw.WriteLine("ExecStartPre=mono /antd/Antd/Antd.exe");
                    sw.WriteLine("ExecStart=/usr/bin/mono /antd/Antd/start.antd.sh");
                    sw.WriteLine("");
                    sw.WriteLine("[Install]");
                    sw.WriteLine("Alias=antd.service");
                    sw.WriteLine("");
                }
            }
            Command.Launch("chmod", "777 " + path);
        }
    }
}