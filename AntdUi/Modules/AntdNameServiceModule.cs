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
using antdlib.common;
using antdlib.config;
using antdlib.models;
using Nancy;
using Newtonsoft.Json;

namespace AntdUi.Modules {
    public class AntdNameServiceModule : NancyModule {

        private readonly ApiConsumer _api = new ApiConsumer();

        public AntdNameServiceModule() {
            Get["/nameservice"] = x => {
                var model = _api.Get<PageNameServiceModel>($"http://127.0.0.1:{Application.ServerPort}/nameservice");
                var json = JsonConvert.SerializeObject(model);
                return json;
            };

            Post["/nameservice/hosts"] = x => {
                string hosts = Request.Form.Hosts;
                var dict = new Dictionary<string, string> {
                    { "Hosts", hosts }
                };
                return _api.Post($"http://127.0.0.1:{Application.ServerPort}/nameservice/hosts", dict);
            };

            Post["/nameservice/networks"] = x => {
                string networks = Request.Form.Networks;
                var dict = new Dictionary<string, string> {
                    { "Networks", networks }
                };
                return _api.Post($"http://127.0.0.1:{Application.ServerPort}/nameservice/hosts", dict);
            };

            Post["/nameservice/resolv"] = x => {
                string resolv = Request.Form.Resolv;
                var dict = new Dictionary<string, string> {
                    { "Resolv", resolv }
                };
                return _api.Post($"http://127.0.0.1:{Application.ServerPort}/nameservice/resolv", dict);
            };

            Post["/nameservice/switch"] = x => {
                string @switch = Request.Form.Switch;
                var dict = new Dictionary<string, string> {
                    { "Switch", @switch }
                };
                return _api.Post($"http://127.0.0.1:{Application.ServerPort}/nameservice/switch", dict);
            };

            Post["/host/int/domain"] = x => {
                string domain = Request.Form.Domain;
                var dict = new Dictionary<string, string> {
                    { "Domain", domain }
                };
                return _api.Post($"http://127.0.0.1:{Application.ServerPort}/host/int/domain", dict);
            };

            Post["/host/ext/domain"] = x => {
                string domain = Request.Form.Domain;
                var dict = new Dictionary<string, string> {
                    { "Domain", domain }
                };
                return _api.Post($"http://127.0.0.1:{Application.ServerPort}/host/ext/domain", dict);
            };
        }
    }
}