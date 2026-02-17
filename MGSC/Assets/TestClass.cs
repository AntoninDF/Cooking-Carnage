using UnityEngine;
using SourceGen;
using System;
using System.Collections.Generic;
namespace TestNamespace
{
	public class MyClass : MonoBehaviour, IEmptyTest
	{
		public int TestInt = 5;
		public int MyIntProperty => 5;
		public SourceGenerator MyOtherProperty { get; set;}
		public Action Action { get; set;}
		public Action<int, string> GenericAction { get; set;}
		public int MyIntField;
		public Color MyOtherField = Color.white;
	}
}
