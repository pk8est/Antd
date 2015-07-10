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
using System.IO;

namespace Antd {

    public class Label {
        public static string Root { get { return "antd_root"; } }
        public static string Port { get { return "antd_port"; } }
        public static string Database { get { return "antd_database"; } }
        public static string Files { get { return "antd_files"; } }
    }

    public class Folder {
        private class tmp {
            public static string Root { get { return AppDomain.CurrentDomain.BaseDirectory; } }
            public static string Config { get { return Path.Combine(Folder.Root, "config"); } }
            public static string Database { get { return Path.Combine(Folder.Root, "database"); } }
            public static string FileRepository { get { return Path.Combine(Folder.Root, "files"); } }
            public static string Networkd { get { return Path.Combine(Folder.Root, "networkd"); } }
        }
        public static string Root { get { return "/cfg/antd"; } }
        public static string Config { get { return Folder.Root + "/config"; } }
        public static string Database { get { return Folder.Root + "/database"; } }
        public static string FileRepository { get { return Folder.Root + "/files"; } }
        public static string Networkd { get { return Folder.Root + "/networkd"; } }
        public static string Apps { get { return "/mnt/cdrom/Apps"; } }
        public static string AppsUnits { get { return "/mnt/cdrom/Units/applicative.target.wants"; } }
    }

    public class UID {
        public static string Guid { get { return System.Guid.NewGuid().ToString(); } }
        public static string ShortGuid { get { return System.Guid.NewGuid().ToString().Substring(0, 8); } }
    }

    public class Port {
        public static string Antd { get { return "7777"; } }
    }

    public class Uri {
        public static string Antd { get { return "http://+:" + Port.Antd + "/"; } }
    }
}