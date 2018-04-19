using System;
using CommandLine;

namespace Io.Autometa.Schema
{
    internal class Options
    {
        [Option('d', "dll", Required = true, HelpText = "DLL to search.")]
        public string Dll { get; set; }

        [Option('t', "type", Required = true, HelpText = "Name of type to search for.")]
        public string Type { get; set; }

        [Option('o', "outDir", Required = true, HelpText = "Directory to write to.")]
        public string OutDirectory { get; set; }
    }
}