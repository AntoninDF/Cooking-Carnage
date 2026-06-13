// <HASHCODE> -1024517996
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
public partial class EventLinker : MonoBehaviour
{
	public List<string> options => 
	new() {
		"Action1",
		"Action2",
		"Action3"
	};
	public UnityEvent Action1;
	public UnityEvent Action2;
	public UnityEvent Action3;
}
