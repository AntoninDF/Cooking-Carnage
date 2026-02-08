using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
public partial class EventLinker : MonoBehaviour
{
	public List<string> options => 
	new() {
		"MyAction",
		"FirstFolder/MyOtherAction"
	};
	public UnityEvent MyAction;
	public UnityEvent MyOtherAction;
}
