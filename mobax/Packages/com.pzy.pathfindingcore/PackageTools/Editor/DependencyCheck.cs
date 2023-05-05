// Disable the warning: "Field 'DependencyCheck.Dependency.name' is never assigned to, and will always have its default value null"
#pragma warning disable 649
using UnityEditor;
using System.Linq;

namespace PathfindingCore.Util {
	[InitializeOnLoad]
	static class DependencyCheck {
		struct Dependency {
			public string name;
			public string version;
		}

		static DependencyCheck() {
			var missingDependencies = new Dependency[] {
#if !MODULE_BURST
				new Dependency {
					name = "com.unity.burst",
					version = "1.0.4",
				},
#endif
#if !MODULE_MATHEMATICS
				new Dependency {
					name = "com.unity.mathematics",
					version = "1.1.0",
				},
#endif
#if !MODULE_COLLECTIONS
				new Dependency {
					name = "com.unity.collections",
					version = "0.1.1",
				},
#endif
#if !MODULE_JOBS
				new Dependency {
					name = "com.unity.jobs",
					version = "0.1.1",
				},
#endif
			};

			if (missingDependencies.Length > 0) {
				string missing = string.Join(", ", missingDependencies.Select(p => p.name + " (" + p.version + ")"));
				bool res = EditorUtility.DisplayDialog("Missing dependencies", "The packages " + missing + " are required by the A* Pathfinding Project but they are not installed, or the installed versions are too old. Do you want to install the latest versions of the packages?", "Ok", "Cancel");
				if (res) {
					foreach (var dep in missingDependencies) {
						UnityEditor.PackageManager.Client.Add(dep.name);
					}
				}
			}
		}
	}
}
