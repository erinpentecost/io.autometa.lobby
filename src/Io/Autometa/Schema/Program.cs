using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NJsonSchema;
using CommandLine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Io.Autometa.Schema
{
    class Program
    {
        static void Main(string[] args)
        {
              CommandLine.Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(opts => Run(opts))
                .WithNotParsed<Options>((errs) => Error(errs));
        }

        private static void Run(Options opt)
        {
            try
            {
                DumpSchema(opt.Dll, opt.Type, opt.OutDirectory).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }

        private static void Error(IEnumerable<CommandLine.Error> err)
        {
            foreach (var e in err)
            {
                Console.Error.WriteLine(e.ToString());
            }
            System.Environment.Exit(1);
        }

        private static async Task DumpSchema(string Dll, string searchedType, string outDir)
        {
            Assembly asm = Assembly.LoadFile(Path.GetFullPath(Dll));

            var foundType = asm.GetTypes()
                .First(f => string.Equals(f.Name, searchedType, StringComparison.InvariantCultureIgnoreCase));

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            var schema = await JsonSchema4.FromTypeAsync(foundType);
            await File.WriteAllTextAsync(Path.Combine(outDir, foundType.Name + ".json"), schema.ToJson());

            // now do types used in methods
            List<Type> derivedTypes = new List<Type>();
            List<MiniMethodInfo> methods = new List<MiniMethodInfo>();
            foreach (var t in foundType.GetMethods())
            {
                var paramTypes = t.GetParameters().Select(p => p.ParameterType).ToList();
                derivedTypes.Add(t.ReturnType);
                derivedTypes.AddRange(paramTypes);

                methods.Append(new MiniMethodInfo(t));
            }
            await File.WriteAllTextAsync(
                Path.Combine(outDir, foundType.Name + ".methods.json"),
                 Newtonsoft.Json.JsonConvert.SerializeObject(methods));

            derivedTypes = derivedTypes.Distinct().ToList();
            foreach (var t in derivedTypes)
            {
                var dSchema = await JsonSchema4.FromTypeAsync(t);
                await File.WriteAllTextAsync(Path.Combine(outDir, t.Name + ".json"), dSchema.ToJson());
            }
        }

        private class MiniMethodInfo
        {
            public string Name {get;set;}
            public string Return {get;set;}
            public List<string> Input {get; set;}
            public MiniMethodInfo(MethodInfo mi)
            {
                this.Name = mi.Name;
                this.Return = mi.ReturnType.Name;
                this.Input = mi.GetParameters().Select(p => p.ParameterType.Name).ToList();
            }
        }
    }
}
