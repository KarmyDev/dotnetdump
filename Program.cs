using System;
using System.IO;
using System.Reflection;

namespace DotNetDumper
{
    class Program
    {
		private static string ansiReset = "\u001b[0m";
		private static string ansiRed = "\u001b[31m";
		private static string ansiBrightRed = "\u001b[31;1m";
		private static string ansiGreen = "\u001b[32m";
		private static string ansiBrightBlue = "\u001b[34;1m";
		private static string ansiYellow = "\u001b[33m";
		private static string ansiBrightMagenta = "\u001b[35;1m";
		private static string ansiBackgroundFind = "\u001b[40m";
		
		private static bool produceGraphviz, findSpecificKeyword, decompileData;
		
        static void Main(string[] args)
        {
			if (args.Length < 1) return;
			
			if (Array.IndexOf(args, "-r") > -1 || Array.IndexOf(args, "--raw") > -1) ansiReset = ansiRed = ansiBrightRed = ansiGreen = ansiBrightBlue = ansiYellow = ansiBrightMagenta = "";
			if (Array.IndexOf(args, "-g") > -1 || Array.IndexOf(args, "--graph") > -1) produceGraphviz = true;
			if (Array.IndexOf(args, "-f") > -1 || Array.IndexOf(args, "--find") > -1) findSpecificKeyword = true;
			if (Array.IndexOf(args, "-d") > -1 || Array.IndexOf(args, "--decompile") > -1) decompileData = true;
			
			if (!produceGraphviz) Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to load {ansiGreen}\"{args[0]}\"{ansiReset}...");
			
			string specificKeyword = "";
			if (args.Length > 1) specificKeyword = args[2];
			
			string pathToAssembly = args[0];
			
			if (!File.Exists(pathToAssembly))
			{
				if (!produceGraphviz) Console.WriteLine($"{ansiRed}X File {ansiGreen}\"{args[0]}\"{ansiRed} doesn't exist.{ansiReset}");
				return;
			}
			
			try 
			{
				Assembly externalDll = Assembly.LoadFrom(pathToAssembly);
				if (!produceGraphviz) Console.WriteLine($"{ansiGreen}• {ansiReset}Loaded {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
				
				if (!produceGraphviz) Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to obtain types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}...");
				try 
				{
					Type[] externalDllTypes = externalDll.GetTypes();
					if (!produceGraphviz)
					Console.WriteLine($"{ansiGreen}• {ansiReset}Successfully obtained types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
					
					if (!produceGraphviz)
					Console.WriteLine("\n");

					string graphHeader = $"digraph G {{\n\t{{\n\t\tbase [label=\"{externalDll.GetName().Name}.dll\", shape=\"folder\"]\n";
					string graphBody = "";
					
					int typeCount = 0;
					bool displayClassContent = false;
					
					foreach (Type type in externalDllTypes)
					{
						if (!produceGraphviz && !findSpecificKeyword || type.Name.ToLower().Contains(specificKeyword.ToLower()))
						Console.WriteLine($"Found Type: {ansiBrightMagenta + (findSpecificKeyword && type.Name.ToLower().Contains(specificKeyword.ToLower()) ? ansiBackgroundFind : "")}{type.Name}{ansiReset}");
						
						displayClassContent = type.Name.ToLower().Contains(specificKeyword.ToLower());
						
						// DECOMPILE
						if (displayClassContent && decompileData) 
						{
							Console.WriteLine("GUID: " + type.GUID.ToString());
							Console.WriteLine("Token: " + type.GetMetadataToken());
							Console.WriteLine("Hash: " + type.GetHashCode() + "\n");
						}
						string graphTableBuilder = "";
						
						foreach (MemberInfo member in type.GetMembers())
						{
							bool isMethod = member.MemberType == MemberTypes.Method;
							string memberColor = isMethod ? ansiBrightBlue : ansiYellow;
							string memberSuffix = isMethod ? "()" : "";
							string memberPrefix = isMethod ? "Method" : "Member";
							
							if (produceGraphviz) 
							{
								string memberGraphColor = isMethod ? "#110ad1" : "#d1780a" ;
								string memberEncodedName = System.Web.HttpUtility.HtmlEncode(member.Name);
								graphTableBuilder += $"<FONT COLOR=\"{memberGraphColor}\">{memberEncodedName + memberSuffix}</FONT><BR/>";
							}
							
							if (!produceGraphviz && !findSpecificKeyword || member.Name.ToLower().Contains(specificKeyword.ToLower()) || displayClassContent)
							Console.WriteLine($"{ansiYellow}-- {ansiReset}Found {memberPrefix}: {memberColor + (findSpecificKeyword && member.Name.ToLower().Contains(specificKeyword.ToLower()) ? ansiBackgroundFind : "")}{member.Name}{ansiReset}{memberSuffix}");
						
							if (displayClassContent && decompileData) 
							{
								Console.WriteLine("Token: " + member.GetMetadataToken());
								Console.WriteLine("Hash: " + member.GetHashCode() + "\n");							
							}
						}
						
						if (produceGraphviz) 
						{
							string typeEncodedName = System.Web.HttpUtility.HtmlEncode(type.Name);
							graphHeader += $"\t\tt{typeCount} [label=<{{<FONT COLOR=\"#29470e\">{typeEncodedName}</FONT>|{graphTableBuilder}}}>, shape=record]\n";
							graphBody += $"\tbase -> t{typeCount} [dir=none]\n";
						};
						
						typeCount++;
					}
					if (!produceGraphviz && !findSpecificKeyword)  Console.WriteLine("\n");
					
					if (produceGraphviz) 
					{
						graphHeader += "\t}\n";
						graphBody += "}";
						Console.WriteLine(graphHeader + graphBody);
					}
					
				}
				catch (Exception ee)
				{
					Console.WriteLine($"{ansiRed}X Failed to load types for {ansiGreen}\"{externalDll.FullName}\"{ansiRed}.{ansiReset}");
					Console.WriteLine($"\n  {ansiRed}Exception: {ansiBrightRed}{ee}{ansiReset}");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"{ansiRed}X Failed to load {ansiGreen}\"{args[0]}\"{ansiRed}.{ansiReset}");
				Console.WriteLine($"\n  {ansiRed}Exception: {ansiBrightRed}{e}{ansiReset}");
				
			}
        }
    }
}

