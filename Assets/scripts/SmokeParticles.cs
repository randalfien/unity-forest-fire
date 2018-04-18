using System.Collections.Generic;
using UnityEngine;

public class SmokeParticles : MonoBehaviour
{

	protected static List<ParticleSystem> AllParticles = new List<ParticleSystem>();
	
	private void Awake()
	{
		AllParticles.Add(GetComponent<ParticleSystem>());
	}

	public static void SetWindSpeed(Vector2 a)
	{
		for (var i = 0; i < AllParticles.Count; i++)
		{
			var sys = AllParticles[i];
			var velocity = sys.velocityOverLifetime;
			velocity.x = a.x;
			velocity.z = a.y;
			
			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[sys.particleCount];
			int count = sys.GetParticles(particles);
			for(int k = 0; k < count; k++)
			{
				var v = particles[k].velocity;
				var age = (particles[k].remainingLifetime / particles[k].startLifetime);
				v.x = age*a.x;
				v.z = age*a.y;
				particles[k].velocity = v;
			}
			
			
 
			sys.SetParticles(particles, count);
		}
	}
}