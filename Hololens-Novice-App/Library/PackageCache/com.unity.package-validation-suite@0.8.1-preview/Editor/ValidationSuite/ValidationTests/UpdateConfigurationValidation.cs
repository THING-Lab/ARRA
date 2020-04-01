using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Compilation;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
#if UNITY_2019_1_OR_NEWER
    internal class UpdateConfigurationValidation : BaseAssemblyValidation
    {
        public UpdateConfigurationValidation()
        {
            this.TestName = "API Updater Configuration Validation";
        }

        protected override bool IncludePrecompiledAssemblies => true;
        protected override void Run(AssemblyInfo[] info)
        {
            this.TestState = TestState.Succeeded;
            if (Context.ProjectPackageInfo?.name == "com.unity.package-validation-suite")
            {
                AddInformation("PackageValidationSuite update configurations tested by editor tests.");
                return;
            }
            
            if (info.Length == 0)
            {
                TestState = TestState.Succeeded;
                return;
            }

            var validatorPath = Path.Combine(EditorApplication.applicationContentsPath, "Tools/ScriptUpdater/APIUpdater.ConfigurationValidator.exe");
            if (!File.Exists(validatorPath))
            {
                AddInformation("APIUpdater.ConfigurationValidator.exe is not present in this version of Unity. Not validating update configurations.");
                return;
            }

            var asmdefAssemblies = info.Where(i => i.assemblyKind == AssemblyInfo.AssemblyKind.Asmdef).ToArray();
            if (asmdefAssemblies.Length > 0)
            {
                var asmdefAssemblyPaths = asmdefAssemblies.Select(i => Path.GetFullPath(i.assembly.outputPath));
                var references = new HashSet<string>(asmdefAssemblies.SelectMany(i => i.assembly.allReferences).Select(Path.GetFullPath));
                RunValidator(references, validatorPath, asmdefAssemblyPaths);
            }

            var precompiledAssemblyInfo = info.Where(i => i.assemblyKind == AssemblyInfo.AssemblyKind.PrecompiledAssembly).ToArray();
            if (precompiledAssemblyInfo.Length > 0)
            {
                var precompiledDllPaths = precompiledAssemblyInfo.Select(i => Path.GetFullPath(i.precompiledDllPath));
                var precompiledAssemblyPaths = CompilationPipeline.GetPrecompiledAssemblyPaths(CompilationPipeline.PrecompiledAssemblySources.All);

                RunValidator(precompiledAssemblyPaths, validatorPath, precompiledDllPaths);
            }
        }

        private void RunValidator(IEnumerable<string> references, string validatorPath, IEnumerable<string> assemblyPaths)
        {
            var responseFilePath = Path.GetTempFileName();
            File.WriteAllLines(responseFilePath, references);

            var monoPath = Utilities.GetMonoPath();
            
            var argumentsForValidator = ArgumentsForValidator();
            File.WriteAllText($"{Path.Combine(Path.GetTempPath(), Context.ProjectPackageInfo?.name)}.updater.validation.arguments", argumentsForValidator);
            
            var processStartInfo = new ProcessStartInfo(monoPath, $@"""{validatorPath}"" {argumentsForValidator}")
            {
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true

            };
            var process = Process.Start(processStartInfo);
            var stderr = new ProcessOutputStreamReader(process, process.StandardError);
            var stdout = new ProcessOutputStreamReader(process, process.StandardOutput);
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                var stdContent = string.Join("\n", stderr.GetOutput().Concat(stdout.GetOutput()));
                if (ApiUpdaterConfigurationExemptions(stdContent))
                    AddWarning(stdContent);
                else
                    AddError(stdContent);
            }

            bool ApiUpdaterConfigurationExemptions(string stdContent)
            {
#if UNITY_2019_3_OR_NEWER
                return false;
#else
                if (stdContent.Contains("Mono.Cecil.AssemblyResolutionException"))
                    return true;

                // This is a temporary workaround to unblock dots team.
                var requiredEntries =  new[] 
                {
                    "Target of update (method ComponentSystemBase.GetEntityQuery) is less accessible than original (Unity.Entities.ComponentGroup Unity.Entities.ComponentSystemBase::GetComponentGroup(Unity.Entities.ComponentType[])).",
                    "Target of update (method ComponentSystemBase.GetEntityQuery) is less accessible than original (Unity.Entities.ComponentGroup Unity.Entities.ComponentSystemBase::GetComponentGroup(Unity.Collections.NativeArray`1<Unity.Entities.ComponentType>)).",
                    "Target of update (method ComponentSystemBase.GetEntityQuery) is less accessible than original (Unity.Entities.ComponentGroup Unity.Entities.ComponentSystemBase::GetComponentGroup(Unity.Entities.EntityArchetypeQuery[])).",
                };

                return requiredEntries.All(stdContent.Contains);
#endif                
            }

            string ArgumentsForValidator()
            {
                var whitelistArg = string.Empty;

                // Resolves ValidationWhiteList.txt in folders
                //      - ApiUpdater~/{Editor Exact Version} (ex: 2019.3.0f1)
                //      - ApiUpdater~/{Editor Version Without Alpha/Beta/RC/Final info} (ex: 2019.3)
                //      - ApiUpdater~/
                // first one found will be used.
                var probingFolders = new [] {$"{UnityEngine.Application.unityVersion}", $"{Regex.Replace(UnityEngine.Application.unityVersion, @"(?<=20[1-5][0-9]\.\d{1,3})\.[0-9]{1,4}.*", string.Empty)}", "."};
                foreach(var path in probingFolders)
                {
                    var whitelistPath = Path.Combine(Context.ProjectPackageInfo.path, $"ApiUpdater~/{path}/ValidationWhiteList.txt");
                    if (File.Exists(whitelistPath))
                    {
                        whitelistArg = $@" --whitelist ""{whitelistPath}""";
                        break;
                    }
                }

                return $"\"{responseFilePath}\" -a {string.Join(",", assemblyPaths.Select(p => $"\"{Path.GetFullPath(p)}\""))} {whitelistArg}";
            }
        }
    }
#endif
}
