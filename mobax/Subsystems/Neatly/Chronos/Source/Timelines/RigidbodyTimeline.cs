using UnityEngine;

namespace Chronos
{
	public interface IRigidbodyTimeline
	{
		bool isKinematic { get; set; }
		float mass { get; set; }
		float drag { get; set; }
		float angularDrag { get; set; }
	}

	public abstract class RigidbodyTimeline<TComponent, TSnapshot> : RecorderTimeline<TComponent, TSnapshot>, IRigidbodyTimeline where TComponent : Component
	{
		public RigidbodyTimeline(Timeline timeline, TComponent component) : base(timeline, component) { }

		public override void Update()
		{
			float timeScale = timeline.timeScale;

			if (lastTimeScale != 0 && timeScale == 0) // Arrived at halt
			{
				if (lastTimeScale > 0)
				{
					zeroSnapshot = CopySnapshot();
				}
				else if (lastTimeScale < 0 && timeline.rewindable)
				{
					zeroSnapshot = interpolatedSnapshot;
				}

				zeroTime = timeline.time;
			}

			if (lastTimeScale >= 0 && timeScale <= 0) // Started pause or rewind
			{
				if (timeScale < 0) // Started rewind
				{
					laterSnapshot = CopySnapshot();
					laterTime = timeline.time;
					interpolatedSnapshot = laterSnapshot;
					canRewind = TryFindEarlierSnapshot(false);
				}

				bodyIsKinematic = true;
			}
			else if (lastTimeScale <= 0 && timeScale > 0) // Stopped pause or rewind
			{
				bodyIsKinematic = isKinematic;
				
				if (lastTimeScale == 0) // Stopped pause
				{
					ApplySnapshot(zeroSnapshot);
				}
				else if (lastTimeScale < 0 && timeline.rewindable) // Stopped rewind
				{
					ApplySnapshot(interpolatedSnapshot);
				}

				WakeUp();

				Record();
			}

			if (timeScale > 0 && timeScale != lastTimeScale && !bodyIsKinematic) // Slowed down or accelerated
			{
				float modifier = timeScale / lastPositiveTimeScale;

				bodyVelocity *= modifier;
				bodyAngularVelocity *= modifier;
				bodyDrag *= modifier;
				bodyAngularDrag *= modifier;

				WakeUp();
			}

			if (timeScale > 0)
			{
				bodyIsKinematic = isKinematic;

				Progress();
			}
			else if (timeScale < 0)
			{
				Rewind();
			}

			lastTimeScale = timeScale;

			if (timeScale > 0)
			{
				lastPositiveTimeScale = timeScale;
			}
		}

		#region Fields

		protected float lastPositiveTimeScale = 1;
		protected TSnapshot zeroSnapshot;
		protected float zeroTime;

		#endregion

		#region Snapshots

		public override void ModifySnapshots(SnapshotModifier modifier)
		{
			base.ModifySnapshots(modifier);

			zeroSnapshot = modifier(zeroSnapshot, zeroTime);
		}

		#endregion

		#region Rigidbody

		protected abstract bool bodyIsKinematic { get; set; }
		protected abstract float bodyMass { get; set; }
		protected abstract Vector3 bodyVelocity { get; set; }
		protected abstract Vector3 bodyAngularVelocity { get; set; }
		protected abstract float bodyDrag { get; set; }
		protected abstract float bodyAngularDrag { get; set; }
		protected abstract bool IsSleeping();
		protected abstract void WakeUp();

		/// <summary>
		/// Determines whether the rigidbody is kinematic before time effects. Use this property instead of Rigidbody.isKinematic, which will be overwritten by the physics timer at runtime. 
		/// </summary>
		public bool isKinematic { get; set; }

		/// <summary>
		/// The mass of the rigidbody before time effects. Use this property instead of Rigidbody.mass, which will be overwritten by the physics timer at runtime. 
		/// </summary>
		public float mass
		{
			// This isn't getting used right now, but leave it here for forward-compatibility
			get { return bodyMass; }
			set { bodyMass = value; }
		}

		/// <summary>
		/// The drag of the rigidbody before time effects. Use this property instead of Rigidbody.drag, which will be overwritten by the physics timer at runtime. 
		/// </summary>
		public float drag
		{
			get { return bodyDrag / timeline.timeScale; }
			set { if (AssertForwardProperty("drag", Severity.Warn)) bodyDrag = value * timeline.timeScale; }
		}

		/// <summary>
		/// The angular drag of the rigidbody before time effects. Use this property instead of Rigidbody.angularDrag, which will be overwritten by the physics timer at runtime. 
		/// </summary>
		public float angularDrag
		{
			get { return bodyAngularDrag / timeline.timeScale; }
			set { if (AssertForwardProperty("angularDrag", Severity.Warn)) bodyAngularDrag = value * timeline.timeScale; }
		}

		protected virtual float AdjustForce(float force)
		{
			return force * timeline.timeScale;
		}

		protected virtual Vector2 AdjustForce(Vector2 force)
		{
			return force * timeline.timeScale;
		}

		protected virtual Vector3 AdjustForce(Vector3 force)
		{
			return force * timeline.timeScale;
		}

		protected bool AssertForwardProperty(string property, Severity severity)
		{
			if (timeline.timeScale <= 0)
			{
				if (severity == Severity.Error)
				{
					throw new ChronosException("Cannot change the " + property + " of the rigidbody while time is paused or rewinding.");
				}
				else if (severity == Severity.Warn)
				{
					Debug.LogWarning("Trying to change the " + property + " of the rigidbody while time is paused or rewinding, ignoring.");
				}
			}

			return timeline.timeScale > 0;
		}

		protected bool AssertForwardForce(Severity severity)
		{
			if (timeline.timeScale <= 0)
			{
				if (severity == Severity.Error)
				{
					throw new ChronosException("Cannot apply a force to the rigidbody while time is paused or rewinding.");
				}
				else if (severity == Severity.Warn)
				{
					Debug.LogWarning("Trying to apply a force to the rigidbody while time is paused or rewinding, ignoring.");
				}
			}

			return timeline.timeScale > 0;
		}

		#endregion
	}
}
