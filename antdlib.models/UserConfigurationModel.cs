﻿using System.Collections.Generic;

namespace antdlib.models {
    public class UserConfigurationModel {
        public List<User> Users { get; set; } = new List<User>();
    }

    public class User {
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
    }
}