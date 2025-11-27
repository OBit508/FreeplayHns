using System;
using System.Diagnostics;

namespace BepInEx.Preloader.Core.Patching
{
	// Token: 0x02000004 RID: 4
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	[Conditional("CodeGeneration")]
	internal sealed class PatcherAutoPluginAttribute : Attribute
	{
		// Token: 0x06000003 RID: 3 RVA: 0x0000206B File Offset: 0x0000026B
		public PatcherAutoPluginAttribute(string id = null, string name = null, string version = null)
		{
		}
	}
}
