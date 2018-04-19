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
                .FirstOrDefault(f => string.Equals(f.Name, searchedType, StringComparison.InvariantCultureIgnoreCase));

            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            if (foundType != default(Type))
            {
                await DumpRootType(foundType, outDir);
            }
            else if (searchedType.EndsWith('*'))
            {
                var st = searchedType.TrimEnd('.', '*');
                var nameSearch = Task.WhenAll(asm.GetTypes()
                .Where(t => st.Equals(t.Namespace, StringComparison.InvariantCultureIgnoreCase))
                .Select(async t => await DumpRootType(t, outDir)).ToArray());
            }
        }

        private static async Task DumpRootType(Type foundType, string outDir)
        {
            var schema = await JsonSchema4.FromTypeAsync(foundType);
            await File.WriteAllTextAsync(Path.Combine(outDir, foundType.GetFriendlyName() + ".json"), schema.ToJson());

            // now do types used in methods
            List<Type> derivedTypes = new List<Type>();
            List<MiniMethodInfo> methods = new List<MiniMethodInfo>();
            foreach (var t in foundType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName))
            {
                var paramTypes = t.GetParameters()
                .Select(p => p.ParameterType)
                .ToList();

                if (t.ContainsGenericParameters)
                {
                    paramTypes.AddRange(t.GetGenericArguments().SelectMany(p => p.GetGenericArguments()));
                }
                
                derivedTypes.Add(t.ReturnType);
                derivedTypes.AddRange(paramTypes);

                methods.Add(new MiniMethodInfo(t));
            }

            if (methods.Count != 0)
            {
                await File.WriteAllTextAsync(
                    Path.Combine(outDir, foundType.GetFriendlyName() + ".methods.json"),
                    Newtonsoft.Json.JsonConvert.SerializeObject(methods, Newtonsoft.Json.Formatting.Indented));
            }

            derivedTypes = derivedTypes.Distinct().Where(t => !t.Namespace.StartsWith("System")).ToList();
            foreach (var t in derivedTypes)
            {
                var dSchema = await JsonSchema4.FromTypeAsync(t);
                await File.WriteAllTextAsync(Path.Combine(outDir, t.GetFriendlyName() + ".json"), dSchema.ToJson());
            }
        }

        public class MiniMethodInfo
        {
            public string Name {get;set;}
            public string Return {get;set;}
            public List<string> Input {get; set;}
            public MiniMethodInfo(MethodInfo mi)
            {
                this.Name = mi.Name;
                this.Return = mi.ReturnType.GetFriendlyName();
                this.Input = mi.GetParameters().Select(p => p.ParameterType.GetFriendlyName()).ToList();
            }
        }
    }
}
