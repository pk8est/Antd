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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Antd.Apps {
    public class Management {
        public class AppInfo {
            public string Name { get; set; }

            public List<KeyValuePair<string, string>> Values { get; set; } = new List<KeyValuePair<string, string>>() { };
        }

        private static string AppsDir = "/mnt/cdrom/Apps";
        public static bool Detect(string searchPattern) {
            if (!Directory.Exists(Folder.Apps)) {
                return false;
            }
            var folders = Directory.GetDirectories(Folder.Apps).ToArray();
            var squashes = Directory.GetFiles(Folder.Apps, searchPattern).ToArray();
            var newArray = folders.Concat(squashes).ToArray();
            if (newArray.Length > 0) {
                return true;
            }
            else {
                return false;
            }
        }

        public static string[] FindFiles() {
            var apps = new List<string>() { };
            var files = Directory.GetFiles(AppsDir);
            if (files.Length < 0) {
                ConsoleLogger.Log("There's no file in {0}", AppsDir);
            }
            else {
                var squashfs = files.Where(i => i.Contains(".squashfs")).ToArray();
                foreach (var s in squashfs) {
                    apps.Add(s.Replace(@"\", "/"));
                }
            }
            return apps.ToArray();
        }

        public static string[] FindDirectories() {
            var apps = new List<string>() { };
            var dirs = Directory.GetDirectories(AppsDir);
            if (dirs.Length < 0) {
                ConsoleLogger.Log("There's no directory in {0}", AppsDir);
            }
            else {
                foreach (var d in dirs) {
                    var f = Directory.GetFiles(Path.GetFullPath(d), "*.appinfo");
                    if (f.Length > 0) {
                        apps.Add(d.Replace(@"\", "/"));
                    }
                }
            }
            return apps.ToArray();
        }

        public static string ReadInfo(string appPath) {
            var f = Directory.GetFiles(Path.GetFullPath(appPath), "*.appinfo").FirstOrDefault();
            return FileSystem.ReadFile(f);
        }

        public static List<string[]> ReadInfoListed(string appPath) {
            var list = new List<string[]>() { };
            var f = Directory.GetFiles(Path.GetFullPath(appPath), "*.appinfo").FirstOrDefault();
            var arr = FileSystem.ReadFile(f).Split(new String[] { @"," }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            foreach (var a in arr) {
                var kv = a.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                if (kv.Length == 2) {
                    list.Add(kv);
                }
            }
            return list;
        }

        public static string[] GetWantedDirectories(string appPath) {
            var list = new List<string>() { };
            foreach (string[] pair in ReadInfoListed(appPath)) {
                if (pair[0] == "path") {
                    list.Add(pair[1]);
                }
            }
            return list.ToArray();
        }

        public static AppInfo[] DetectApps() {
            var list = new List<AppInfo>() { };
            var appinfoFiles = Directory.EnumerateFiles("/framework", "*.appinfo", SearchOption.AllDirectories).ToArray();
            for (int i = 0; i < appinfoFiles.Length; i++) {
                list.Add(MapInfoFile(appinfoFiles[i]));
            }
            return list.ToArray();
        }

        private static AppInfo MapInfoFile(string path) {
            var text = FileSystem.ReadFile(path);
            var rows = text.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var appinfo = new AppInfo();
            foreach (var row in rows) {
                var pair = row.Split(new String[] { ":" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                if (pair.Length > 1) {
                    var kvp = new KeyValuePair<string, string>(pair[0], pair[1]);
                    appinfo.Values.Add(kvp);
                }
            }
            if (appinfo.Values.Count() > 0) {
                appinfo.Name = appinfo.Values.Where(k => k.Key == "name").FirstOrDefault().Value;
            }
            return appinfo;
        }
    }
}
