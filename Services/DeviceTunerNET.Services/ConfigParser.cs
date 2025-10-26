using DeviceTunerNET.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DeviceTunerNET.Services
{
    public class ConfigParser : IConfigParser
    {
        private readonly List<string> _config = [];
        public IEnumerable<string> GetErrorsVariables { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IEnumerable<string> GetConfig() => _config;

        public IConfigParser.Errors Parse(Dictionary<string, string> variables, string templateConfigPath, string outputConfigPath)
        {
            if (!File.Exists(templateConfigPath))
                return IConfigParser.Errors.FileNotFound;

            try
            {
                // Open a stream for the source file
                using (var sourceFile = File.OpenText(templateConfigPath))
                {
                    // Create a temporary file path where we can write modify lines
                    using (StreamWriter outputFileStream = File.CreateText(outputConfigPath))
                    // Open a stream for the temporary file
                    
                    {
                        string line;
                        var sb = new StringBuilder();
                        // read lines while the file has them
                        while ((line = sourceFile.ReadLine()) != null)
                        {
                            // Do the word replacement
                            var newLine = ReplaceByDictionary(variables, line);
                            _config.Add(newLine);
                            // Write the modified line to the new file
                            outputFileStream.WriteLine(newLine);
                        }
                    }
                }
                
                return IConfigParser.Errors.Ok;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                return IConfigParser.Errors.IoException;
            }
        }

        private static string ReplaceByDictionary(Dictionary<string, string> dict, string configLine)
        {
            var result = configLine;

            foreach (var variable in dict)
            {
                result = result.Replace(variable.Key, variable.Value);
            }
                
            return result;
        }
    }
}
