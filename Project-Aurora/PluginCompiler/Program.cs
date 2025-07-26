using System;
using System.IO;
using CSScripting;
using CSScriptLib;

if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: PluginCompiler <script file path>");
    Console.ReadLine();
    return;
}

var path = args.JoinBy(" ");
Console.WriteLine("Compiling...\n" + path);

var outputFile = path + ".dll";
if (File.Exists(outputFile))
{
    File.Delete(outputFile);
}

try
{
    CSScript.RoslynEvaluator.ReferenceAssembly("AuroraRgb.dll");
    CSScript.RoslynEvaluator.CompileAssemblyFromFile(path, outputFile);
}
catch (Exception e)
{
    Console.Error.WriteLine("Error compiling script:");
    Console.Error.WriteLine(e.Message);

    Console.ReadLine();
    throw;
}
