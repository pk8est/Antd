﻿using System;
using System.IO;
using System.Linq;
using antdlib.common;
using Newtonsoft.Json;
using IoDir = System.IO.Directory;

namespace Antd.Rsync {
    public class RsyncConfiguration {

        private readonly RsyncConfigurationModel _serviceModel;

        private readonly string _cfgFile = $"{Parameter.AntdCfgServices}/rsync.conf";
        private readonly string _cfgFileBackup = $"{Parameter.AntdCfgServices}/rsync.conf.bck";
        private  DirectoryWatcher _directoryWatcher;

        public RsyncConfiguration() {
            IoDir.CreateDirectory(Parameter.AntdCfgServices);
            if(!File.Exists(_cfgFile)) {
                _serviceModel = new RsyncConfigurationModel();
            }
            else {
                try {
                    var text = File.ReadAllText(_cfgFile);
                    var obj = JsonConvert.DeserializeObject<RsyncConfigurationModel>(text);
                    _serviceModel = obj;
                }
                catch(Exception) {
                    _serviceModel = new RsyncConfigurationModel();
                }

            }
        }

        public void Save(RsyncConfigurationModel model) {
            var text = JsonConvert.SerializeObject(model, Formatting.Indented);
            if(File.Exists(_cfgFile)) {
                File.Copy(_cfgFile, _cfgFileBackup, true);
            }
            File.WriteAllText(_cfgFile, text);
        }


        public void Set() {
            Enable();
            Restart();
        }

        public bool IsActive() {
            if(!File.Exists(_cfgFile)) {
                return false;
            }
            return _serviceModel != null && _serviceModel.IsActive;
        }

        public RsyncConfigurationModel Get() {
            return _serviceModel;
        }

        public void Enable() {
            if(_serviceModel == null) {
                return;
            }
            _serviceModel.IsActive = true;
            Save(_serviceModel);
        }

        public void Disable() {
            if(_serviceModel == null) {
                return;
            }
            _serviceModel.IsActive = false;
            Save(_serviceModel);
        }

        public void Stop() {
            _directoryWatcher.Stop();
        }

        public void Restart() {
            Stop();
            _directoryWatcher = new DirectoryWatcher(_serviceModel.Directories.ToArray());
            _directoryWatcher.StartWatching();
        }

        public void AddDirectory(RsyncDirectoriesModel model) {
            if(_serviceModel == null) {
                return;
            }
            var dirs = _serviceModel.Directories;
            dirs.Add(model);
            _serviceModel.Directories = dirs;
            Save(_serviceModel);
        }

        public void RemoveDirectory(string guid) {
            if(_serviceModel == null) {
                return;
            }
            var dirs = _serviceModel.Directories;
            var model = dirs.First(_ => _.Guid == guid);
            if(model == null) {
                return;
            }
            dirs.Remove(model);
            _serviceModel.Directories = dirs;
            Save(_serviceModel);
        }
    }
}
