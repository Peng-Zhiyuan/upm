using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Examples.RTS {
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_r_t_s_1_1_r_t_s_audio.php")]
	public class RTSAudio : VersionedMonoBehaviour {
		List<Source> sources = new List<Source>();

		class Source {
			public AudioSource source;

			public bool available {
				get {
					return !source.isPlaying;
				}
			}

			public void Play (AudioClip clip) {
				source.PlayOneShot(clip);
			}
		}

		Source GetSource () {
			for (int i = 0; i < sources.Count; i++) {
				if (sources[i].available) {
					return sources[i];
				}
			}

			var go = new GameObject("Source");
			go.transform.SetParent(transform, false);
			var source = new Source {
				source = go.AddComponent<AudioSource>()
			};

			sources.Add(source);
			return source;
		}

		public void Play (AudioClip clip) {
			GetSource().Play(clip);
		}
	}
}
