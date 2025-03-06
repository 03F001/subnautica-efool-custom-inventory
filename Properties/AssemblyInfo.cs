using System.Reflection;
using System.Runtime.InteropServices;

using org.efool.subnautica.custom_inventory;

[assembly: AssemblyTitle(Info.title)]
[assembly: AssemblyDescription(Info.desc)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("efool")]
[assembly: AssemblyProduct(Info.name)]
[assembly: AssemblyCopyright("Copyright 2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion(Info.version)]
[assembly: AssemblyFileVersion(Info.version)]

[assembly: ComVisible(false)]

[assembly: Guid("4b29aa48-0613-4c23-aabc-6cc9e516a2f2")]

namespace org.efool.subnautica.custom_inventory {
public static class Info
{
	public const string FQN = "org.efool.subnautica.custom_inventory";
	public const string name = "efool-custom-inventory";
	public const string title = "efool's Custom Inventory";
	public const string desc = "efool's custom inventory mod for Subnautica";
	public const string version = "0.0.1";
}
}