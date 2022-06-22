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
		
		private static bool produceGraphviz;
		
        static void Main(string[] args)
        {
			if (args.Length < 1) return;
			
			if (Array.IndexOf(args, "-r") > -1 || Array.IndexOf(args, "--raw") > -1) ansiReset = ansiRed = ansiBrightRed = ansiGreen = ansiBrightBlue = ansiYellow = ansiBrightMagenta = "";
			if (Array.IndexOf(args, "-g") > -1 || Array.IndexOf(args, "--graph") > -1) produceGraphviz = true;
			
			if (!produceGraphviz) Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to load {ansiGreen}\"{args[0]}\"{ansiReset}...");
			
			string pathToAssembly = args[0];
			
			if (!File.Exists(pathToAssembly))
			{
				if (!produceGraphviz)Console.WriteLine($"{ansiRed}X File {ansiGreen}\"{args[0]}\"{ansiRed} doesn't exist.{ansiReset}");
				return;
			}
			
			try 
			{
				Assembly externalDll = Assembly.LoadFrom(pathToAssembly);
				if (!produceGraphviz)Console.WriteLine($"{ansiGreen}• {ansiReset}Loaded {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
				
				if (!produceGraphviz)Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to obtain types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}...");
				try 
				{
					Type[] externalDllTypes = externalDll.GetTypes();
					if (!produceGraphviz)Console.WriteLine($"{ansiGreen}• {ansiReset}Successfully obtained types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
					
					if (!produceGraphviz)Console.WriteLine("\n");

					string graphHeader = $"digraph G {{\n\t{{\n\t\tbase [label=\"{externalDll.GetName().Name}.dll\", shape=\"folder\"]\n";
					string graphBody = "";
					
					foreach (Type type in externalDllTypes)
					{
						if (!produceGraphviz)Console.WriteLine($"Found Type: {ansiBrightMagenta}{type.Name}{ansiReset}");
						if (produceGraphviz) 
						{
							graphHeader += $"\t\tt_{type.Name} [label=\"{type.Name}\", shape=component, color=darkgreen]\n";
							graphBody += $"\tbase -> t_{type.Name} [dir=none]\n";
						};
						
						foreach (MemberInfo member in type.GetMembers())
						{
							bool isMethod = member.MemberType == MemberTypes.Method;
							string memberColor = isMethod ? ansiBrightBlue : ansiYellow;
							string memberSuffix = isMethod ? "()" : "";
							string memberPrefix = isMethod ? "Method" : "Member";
							
							if (produceGraphviz) 
							{
								string memberGraphShape = isMethod ? "cds" : "signature" ;
								string memberGraphColor = isMethod ? "blue" : "red" ;
								graphHeader += $"\t\tm_{type.Name}_{member.Name.Replace('.', '_')} [label=\"{member.Name + memberSuffix}\", shape=\"{memberGraphShape}\", color={memberGraphColor}]\n";
								graphBody += $"\tt_{type.Name} -> m_{type.Name}_{member.Name.Replace('.', '_')} [dir=none]\n";
							}
							if (!produceGraphviz)Console.WriteLine($"{ansiYellow}-- {ansiReset}Found {memberPrefix}: {memberColor}{member.Name}{ansiReset}{memberSuffix}");
						}
						
						if (produceGraphviz) graphHeader += "\t}\n";
					}
					if (!produceGraphviz)Console.WriteLine("\n");
					
					if (produceGraphviz) 
					{
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

