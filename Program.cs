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
		
        static void Main(string[] args)
        {
			if (args.Length < 1) return;
			
			if (Array.IndexOf(args, "-r") > -1 || Array.IndexOf(args, "--raw") > -1) ansiReset = ansiRed = ansiBrightRed = ansiGreen = ansiBrightBlue = ansiYellow = ansiBrightMagenta = "";
			
			Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to load {ansiGreen}\"{args[0]}\"{ansiReset}...");
			
			string pathToAssembly = args[0];
			
			if (!File.Exists(pathToAssembly))
			{
				Console.WriteLine($"{ansiRed}X File {ansiGreen}\"{args[0]}\"{ansiRed} doesn't exist.{ansiReset}");
				return;
			}
			
			try 
			{
				Assembly externalDll = Assembly.LoadFrom(pathToAssembly);
				Console.WriteLine($"{ansiGreen}• {ansiReset}Loaded {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
				
				Console.WriteLine($"{ansiYellow}? {ansiReset}Attempting to obtain types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}...");
				try 
				{
					Type[] externalDllTypes = externalDll.GetTypes();
					Console.WriteLine($"{ansiGreen}• {ansiReset}Successfully obtained types from {ansiGreen}\"{externalDll.FullName}\"{ansiReset}!");
					
					Console.WriteLine("\n");
					foreach (Type type in externalDllTypes)
					{
						Console.WriteLine($"Found Type: {ansiBrightMagenta}{type.Name}{ansiReset}");
						
						foreach (MemberInfo member in type.GetMembers())
						{
							bool isMethod = member.MemberType == MemberTypes.Method;
							string memberColor = isMethod ? ansiBrightBlue : ansiYellow;
							string memberSuffix = isMethod ? "()" : "" ;
							string memberPrefix = isMethod ? "Method" : "Member" ;
							
							Console.WriteLine($"{ansiYellow}-- {ansiReset}Found {memberPrefix}: {memberColor}{member.Name}{ansiReset}{memberSuffix}");
						}
					}
					Console.WriteLine("\n");
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

