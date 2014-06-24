using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Wolfje.Plugins.SEconomy.Performance {
    internal class Profiler {

        internal class Profile {
            public string Name { get; set; }
            public Guid ID { get; set; }
            public Stopwatch Stopwatch { get; set; }
        }

        List<Profile> _profilerTable;

        public Guid Enter(string Name) {
            if (!SEconomyPlugin.Configuration.EnableProfiler) {
                return Guid.Empty;
            }

            if (_profilerTable == null) {
                _profilerTable = new List<Profile>(100);
            }
            Profile profile = new Profile() { ID = Guid.NewGuid(), Name = Name, Stopwatch = Stopwatch.StartNew() };
            _profilerTable.Add(profile);

            return profile.ID;
        }

        public KeyValuePair<string, long>? Exit(Guid id) {
            Profile profile = _profilerTable.SingleOrDefault(i => i.ID == id);

            if (profile != null) {
                Stopwatch value = profile.Stopwatch;
                value.Stop();

                _profilerTable.Remove(profile);
                return new KeyValuePair<string, long>(profile.Name, value.ElapsedMilliseconds);
            }

            return null;
        }

        public string ExitString(Guid id) {
            if (!SEconomyPlugin.Configuration.EnableProfiler) {
                return "";
            }

            var profile = Exit(id);

            if (profile.HasValue) {
                return string.Format("profiler: {0} took {1}ms.", profile.Value.Key, profile.Value.Value);
            } else {
                return null;
            }
        }

        public void ExitLog(Guid id) {
            if (!SEconomyPlugin.Configuration.EnableProfiler) {
                return;
            }

            var profile = Exit(id);

            if (profile.HasValue) {
                TShockAPI.Log.ConsoleInfo(string.Format("profiler: {0} took {1}ms.", profile.Value.Key, profile.Value.Value));
            }
        }

    }
}
