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

using System;
using System.Collections.Generic;
using SysFile = System.IO.File;

namespace antdlib.common {

    public class Write {

        public IEnumerable<string> WriteFile(string path, string text, bool overwrite = true) {
            try {
                if(SysFile.Exists(path) && overwrite) {
                    SysFile.Delete(path);
                    SysFile.WriteAllText(path, text);
                    return new List<string>();
                }
                if(SysFile.Exists(path) && !overwrite) {
                    return new List<string>();
                }
                SysFile.WriteAllText(path, text);
                return new List<string>();
            }
            catch(Exception) {
                return new List<string>();
            }
        }

        public IEnumerable<string> WriteFileLines(string path, IEnumerable<string> text, bool overwrite = true) {
            try {
                if(SysFile.Exists(path) && overwrite) {
                    SysFile.Delete(path);
                    SysFile.WriteAllLines(path, text);
                    return new List<string>();
                }
                if(SysFile.Exists(path) && !overwrite) {
                    return new List<string>();
                }
                SysFile.WriteAllLines(path, text);
                return new List<string>();
            }
            catch(Exception) {
                return new List<string>();
            }
        }

        public IEnumerable<string> AppendFile(string path, string text) {
            try {
                if(!SysFile.Exists(path)) {
                    return new List<string>();
                }
                SysFile.AppendAllText(path, text);
                return new List<string>();
            }
            catch(Exception) {
                return new List<string>();
            }
        }

        public IEnumerable<string> AppendFileLines(string path, IEnumerable<string> text) {
            try {
                if(!SysFile.Exists(path)) {
                    return new List<string>();
                }
                SysFile.AppendAllLines(path, text);
                return new List<string>();
            }
            catch(Exception) {
                return new List<string>();
            }
        }
    }
}