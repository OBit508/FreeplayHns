using System;
using System.Diagnostics;

namespace BepInEx
{
	// Token: 0x02000003 RID: 3
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[Conditional("CodeGeneration")]
	internal sealed class BepInAutoPluginAttribute : Attribute
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002061 File Offset: 0x00000261
		public BepInAutoPluginAttribute(string id = null, string name = null, string version = null)
		{
		}
	}
}
