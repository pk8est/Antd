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
using System.Linq;
using antdlib.views;
using Antd.Apps;
using Antd.Database;
using Antd.Gluster;
using Antd.Log;
using Nancy;
using Nancy.Security;

namespace Antd.Modules {
    public class HomeModule : CoreModule {

        private static readonly ApplicationRepository ApplicationRepository = new ApplicationRepository();
        private readonly AppsManagement _appsManagement = new AppsManagement();
        private readonly GlusterConfiguration _glusterConfiguration = new GlusterConfiguration();

        public HomeModule() {
            this.RequiresAuthentication();

            After += ctx => {
                if(ctx.Response.ContentType == "text/html") {
                    ctx.Response.ContentType = "text/html; charset=utf-8";
                }
            };

            Get["/"] = x => {
                dynamic vmod = new ExpandoObject();
                return View["antd/page-antd", vmod];
            };

            Get["/apps"] = x => {
                dynamic vmod = new ExpandoObject();
                vmod.Detected = _appsManagement.Detect();
                vmod.AppList = ApplicationRepository.GetAll().Select(_ => new ApplicationModel(_));
                return View["antd/page-apps", vmod];
            };

            Get["/ca"] = x => {
                dynamic viewModel = new ExpandoObject();
                return View["antd/page-ca", viewModel];
            };

            Get["/vnc"] = x => {
                dynamic vmod = new ExpandoObject();
                vmod.Connections = new Dictionary<string, string>();
                return View["antd/page-vnc", vmod];
            };

            Get["/boot"] = x => {
                dynamic vmod = new ExpandoObject();
                return View["antd/page-boot", vmod];
            };

            Get["/asset"] = x => {
                dynamic vmod = new ExpandoObject();
                return View["antd/page-asset", vmod];
            };

            Get["/log"] = x => {
                dynamic viewModel = new ExpandoObject();
                return View["antd/page-log", viewModel];
            };

            Post["/gluster/set"] = x => {
                string name = Request.Form.Name;
                string path = Request.Form.Path;
                string nodes = Request.Form.Node;
                var nodelist = nodes.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                string volumeNames = Request.Form.GlusterVolumeName;
                string volumeBrick = Request.Form.GlusterVolumeBrick;
                string volumeMountPoint = Request.Form.GlusterVolumeMountPoint;
                var volumeNamesList = volumeNames.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var volumeBrickList = volumeBrick.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var volumeMountPointList = volumeMountPoint.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var volumelist = new List<GfsVolume>();
                for(var i = 0; i < 20; i++) {
                    if(volumeNamesList.Length < i || volumeBrickList.Length < i || volumeMountPointList.Length < i) { continue; }
                    var vol = new GfsVolume {
                        Name = volumeNamesList[i],
                        Brick = volumeBrickList[i],
                        MountPoint = volumeMountPointList[i],
                    };
                    volumelist.Add(vol);
                }
                var setup = new GlusterSetup {
                    Name = name,
                    Path = path,
                    IsConfigured = true,
                    Nodes = nodelist,
                    Volumes = volumelist
                };
                _glusterConfiguration.Set(setup);
                _glusterConfiguration.Launch();
                return Response.AsRedirect("/");
            };
        }
    }
}