using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoDestructVFX : MonoBehaviour
{
    // If true, deactivate the object instead of destroying it
    public bool OnlyDeactivate;
	
    void OnEnable()
    {
        StartCoroutine(nameof(CheckIfAlive));
    }
	
    IEnumerator CheckIfAlive ()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
		
        while(ps != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (ps.IsAlive(true)) continue;
            if(OnlyDeactivate)
            {
#if UNITY_3_5
						gameObject.SetActiveRecursively(false);
					#else
                gameObject.SetActive(false);
#endif
            }
            else
                Destroy(gameObject);
            break;
        }
    }
}
