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
using System.Dynamic;
using System.IO;
using antdlib;
using antdlib.common;
using Antd.Database;
using Nancy;
using Nancy.Security;

namespace Antd.Modules {

    public class AuthorizedKeysModule : CoreModule {

        private readonly AuthorizedKeysRepository _authorizedKeysRepository = new AuthorizedKeysRepository();

        public AuthorizedKeysModule() {
            this.RequiresAuthentication();

            Get["/page/ak"] = x => {
                dynamic vmod = new ExpandoObject();
                vmod.Keys = _authorizedKeysRepository.GetAll();
                return View["page-ak", vmod];
            };

            Post["/page/ak"] = x => {
                string remoteUser = Request.Form.RemoteUser;
                string user = Request.Form.User;
                string key = Request.Form.Key;
                _authorizedKeysRepository.Create2(remoteUser, user, key);
                return Response.AsRedirect("/page/ak");
            };

            Post["/ak/create"] = x => {
                string remoteUser = Request.Form.RemoteUser;
                string user = Request.Form.User;
                string key = Request.Form.Key;
                var r = _authorizedKeysRepository.Create2(remoteUser, user, key);

                var home = user == "root" ? "/root/.ssh" : $"/home/{user}/.ssh";
                var authorizedKeysPath = $"{home}/authorized_keys";
                if (!File.Exists(authorizedKeysPath)) {
                    File.Create(authorizedKeysPath);
                }
                var line = $"{key} {remoteUser}";
                File.AppendAllLines(authorizedKeysPath, new List<string> { line });
                Bash.Execute($"chmod 600 {authorizedKeysPath}");
                Bash.Execute($"chown {user}:{user} {authorizedKeysPath}");

                return r ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            };

            #region for remote handshaking with avahi discovery
            Post["/ak/handshake"] = x => {
                ConsoleLogger.Log("hs > request");
                string apple = Request.Form.ApplePie;
                if (string.IsNullOrEmpty(apple)) {
                    ConsoleLogger.Log("hs > no key");
                    return HttpStatusCode.InternalServerError;
                }

                var info = apple.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (info.Length < 2) {
                    ConsoleLogger.Log("hs > no info");
                    return HttpStatusCode.InternalServerError;
                }

                var key = info[0];
                var remoteUser = info[1];
                const string user = "root";
                var r = _authorizedKeysRepository.Create2(remoteUser, user, key);
                ConsoleLogger.Log("hs > keys registered");

                const string authorizedKeysPath = "/root/.ssh/authorized_keys";
                if (!File.Exists(authorizedKeysPath)) {
                    File.Create(authorizedKeysPath);
                }
                var line = $"{key} {remoteUser}";
                File.AppendAllLines(authorizedKeysPath, new List<string> { line });
                Bash.Execute($"chmod 600 {authorizedKeysPath}");
                Bash.Execute($"chown {user}:{user} {authorizedKeysPath}");
                ConsoleLogger.Log("hs > keys inserted");

                return r ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            };
            #endregion for remote handshaking with avahi discovery

            Get["/ak/introduce"] = x => {
                var remoteHost = Request.Query.Host;
                var remoteUser = $"{Environment.UserName}@{Environment.MachineName}";
                Console.WriteLine(remoteUser);
                string user = Request.Query.User;
                string key = Request.Query.Key;
                var dict = new Dictionary<string, string> {
                    {"RemoteUser", remoteUser},
                    {"User", user},
                    {"Key", key}
                };
                var r = new ApiConsumer().Post($"http://{remoteHost}/ak/create", dict);
                return r;
            };

            Post["/ak/introduce"] = x => {
                var remoteHost = Request.Form.Host;
                var remoteUser = $"{Environment.UserName}@{Environment.MachineName}";
                Console.WriteLine(remoteUser);
                string user = Request.Form.User;
                string key = Request.Form.Key;
                var dict = new Dictionary<string, string> {
                    {"RemoteUser", remoteUser},
                    {"User", user},
                    {"Key", key}
                };
                var r = new ApiConsumer().Post($"http://{remoteHost}/ak/create", dict);
                return r;
            };
        }
    }
}