using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CommandLine;

namespace DotNetDumper
{
    class Program
    {
		public class Options
		{
			[Value(0, Required = true, MetaName = "path", HelpText = "File to dump.")]
			public string PathToAssembly {get; set;}
			
			[Option('r', "raw", Default = false, Required = false, HelpText = "Produce raw data. Containing ansi-colorless text.")]
			public bool RawData {get; set;}
			[Option('g', "graph", Default = false, Required = false, HelpText = "Produce graphviz representation of dumped data.")]
			public bool ProduceGraphviz {get; set;}
			[Option('f', "find", Default = "", Required = false, HelpText = "Find class/method inside of dumped data.")]
			public string FindSpecificKeyword {get; set;}
			[Option('m', "more", Default = false, Required = false, HelpText = "Get more in-depth informations out of class/method inside of dumped data.")]
			public bool InDepthInfo {get; set;}
		}
		
		private static string ansiReset = "\u001b[0m";
		private static string ansiRed = "\u001b[31m";
		private static string ansiBrightRed = "\u001b[31;1m";
		private static string ansiGreen = "\u001b[32m";
		private static string ansiBrightBlue = "\u001b[34;1m";
		private static string ansiYellow = "\u001b[33m";
		private static string ansiBrightMagenta = "\u001b[35;1m";
		private static string ansiBackgroundFind = "\u001b[40m";
		
		static void RunOptions(Options opts)
		{
			if (!opts.ProduceGraphviz) Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to load {ansiGreen}\"{opts.PathToAssembly}\"{ansiReset}...");
			
			if (!File.Exists(opts.PathToAssembly))
			{
				if (!opts.ProduceGraphviz) Console.WriteLine($"{ansiRed}X File {ansiGreen}\"{opts.PathToAssembly}\"{ansiRed} doesn't exist.{ansiReset}");
				return;
			}
			
			try 
			{
				Assembly externalDll = Assembly.LoadFrom(opts.PathToAssembly);
				if (!opts.ProduceGraphviz) Console.WriteLine($"{ansiGreen}• {ansiReset}Loaded {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
				
				if (!opts.ProduceGraphviz) Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to obtain types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}...");
				try 
				{
					Type[] externalDllTypes = externalDll.GetTypes();
					if (!opts.ProduceGraphviz)
					Console.WriteLine($"{ansiGreen}• {ansiReset}Successfully obtained types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
					
					if (!opts.ProduceGraphviz)
					Console.WriteLine("\n");

					string graphHeader = $"digraph G {{\n\t{{\n\t\tbase [label=\"{externalDll.GetName().Name}.dll\", shape=\"folder\"]\n";
					string graphBody = "";
					
					int typeCount = 0;
					bool displayClassContent = false;
					
					foreach (Type type in externalDllTypes)
					{
						if (!opts.ProduceGraphviz && string.IsNullOrEmpty(opts.FindSpecificKeyword) || type.Name.ToLower().Contains(opts.FindSpecificKeyword.ToLower()))
						Console.WriteLine($"Found Class: {ansiBrightMagenta + (!string.IsNullOrEmpty(opts.FindSpecificKeyword) && type.Name.ToLower().Contains(opts.FindSpecificKeyword.ToLower()) ? ansiBackgroundFind : "")}{type.Name}{ansiReset}");
						
						displayClassContent = type.Name.ToLower().Contains(opts.FindSpecificKeyword.ToLower());
						
						// IN-DEPTH
						if (displayClassContent && opts.InDepthInfo) 
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
							
							string parameters = "";
							string returnType = "";
							if (isMethod)
							{
								ParameterInfo[] ps = ((MethodInfo) member).GetParameters();
								
								for (int i = 0; i < ps.Length; i++)
								{
									parameters += $"{ansiRed}{ps[i].ParameterType.Name}{ansiReset} {ps[i].Name}";
									if (i != ps.Length - 1) parameters += ", ";
								}
								
								returnType = $"{ansiYellow} -> {ansiRed}{((MethodInfo)member).ReturnType.Name}{ansiReset}";
							}
							
							string memberSuffix = isMethod ? $"({parameters})" : "";
							string memberPrefix = isMethod ? "Method" : "Member";
							
							if (opts.ProduceGraphviz) 
							{
								string memberGraphColor = isMethod ? "#110ad1" : "#d1780a" ;
								string memberEncodedName = System.Web.HttpUtility.HtmlEncode(member.Name);
								graphTableBuilder += $"<FONT COLOR=\"{memberGraphColor}\">{memberEncodedName + memberSuffix}</FONT><BR/>";
							}
							
							if (
								!opts.ProduceGraphviz 
								&& string.IsNullOrEmpty(opts.FindSpecificKeyword) 
								|| member.Name.ToLower().Contains(opts.FindSpecificKeyword.ToLower()) 
								|| displayClassContent
							)
							Console.WriteLine($"{ansiYellow}-- {ansiReset}Found {memberPrefix}: {memberColor + (!string.IsNullOrEmpty(opts.FindSpecificKeyword) && member.Name.ToLower().Contains(opts.FindSpecificKeyword.ToLower()) ? ansiBackgroundFind : "")}{member.Name}{ansiReset}{memberSuffix}{returnType}");
						
							if (displayClassContent && opts.InDepthInfo) 
							{
								Console.WriteLine("Token: " + member.GetMetadataToken());
								Console.WriteLine("Hash: " + member.GetHashCode() + "\n");							
							}
						}
						
						if (opts.ProduceGraphviz) 
						{
							string typeEncodedName = System.Web.HttpUtility.HtmlEncode(type.Name);
							graphHeader += $"\t\tt{typeCount} [label=<{{<FONT COLOR=\"#29470e\">{typeEncodedName}</FONT>|{graphTableBuilder}}}>, shape=record]\n";
							graphBody += $"\tbase -> t{typeCount} [dir=none]\n";
						};
						
						typeCount++;
					}
					if (!opts.ProduceGraphviz && string.IsNullOrEmpty(opts.FindSpecificKeyword)) Console.WriteLine("\n");
					
					if (opts.ProduceGraphviz) 
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
				Console.WriteLine($"{ansiRed}X Failed to load {ansiGreen}\"{opts.PathToAssembly}\"{ansiRed}.{ansiReset}");
				Console.WriteLine($"\n  {ansiRed}Exception: {ansiBrightRed}{e}{ansiReset}");
				
			}
		}
		
		static void HandleParseError(IEnumerable<Error> errs)
		{
			foreach (Error error in errs)
			{
				Console.WriteLine($"{ansiRed}(X) {error}");
			}
		}
		
        static void Main(string[] args)
        {
			Parser.Default.ParseArguments<Options>(args).WithParsed(RunOptions).WithNotParsed(HandleParseError);
		}
		
	}
}

