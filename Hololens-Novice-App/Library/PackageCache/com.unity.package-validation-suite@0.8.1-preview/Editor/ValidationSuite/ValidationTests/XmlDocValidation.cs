using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class XmlDocValidation : BaseAssemblyValidation
    {
        public XmlDocValidation()
        {
            Init();
        }

        public XmlDocValidation(ValidationAssemblyInformation validationAssemblyInformation)
            : base(validationAssemblyInformation)
        {
            Init();
        }

        private void Init()
        {
            TestName = "Xmldoc Validation";
            TestDescription = "Checks public API to ensure xmldocs exist.";
            TestCategory = TestCategory.ApiValidation;
            SupportedValidations = new[] { ValidationType.CI, ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.Publishing };
        }
        protected override void Run(AssemblyInfo[] info)
        {
            TestState = TestState.Succeeded;
            var monopath = Utilities.GetMonoPath();
            var exePath = Path.GetFullPath("packages/com.unity.package-validation-suite/Bin~/FindMissingDocs/FindMissingDocs.exe");

            List<string> excludePaths = new List<string>();
            excludePaths.AddRange(Directory.GetDirectories(this.Context.ProjectPackageInfo.path, "*~", SearchOption.AllDirectories));
            excludePaths.AddRange(Directory.GetDirectories(this.Context.ProjectPackageInfo.path, ".*", SearchOption.AllDirectories));
            excludePaths.AddRange(Directory.GetDirectories(this.Context.ProjectPackageInfo.path, "Tests", SearchOption.AllDirectories));
            foreach (var assembly in info)
            {
                //exclude sources from test assemblies explicitly. Do not exclude entire directories, as there may be nested public asmdefs
                if (validationAssemblyInformation.IsTestAssembly(assembly) && assembly.assemblyKind == AssemblyInfo.AssemblyKind.Asmdef)
                    excludePaths.AddRange(assembly.assembly.sourceFiles);
            }
            string responseFileParameter = string.Empty;
            string responseFilePath = null;
            if (excludePaths.Count > 0)
            {
                responseFilePath = Path.GetTempFileName();
                var excludedPathsParameter = $@"--excluded-paths=""{string.Join(",", excludePaths.Select(s => Path.GetFullPath(s)))}""";
                File.WriteAllText(responseFilePath, excludedPathsParameter);
                responseFileParameter = $@"--response-file=""{responseFilePath}""";
            }

            var startInfo = new ProcessStartInfo(monopath, $@"""{exePath}"" --root-path=""{this.Context.ProjectPackageInfo.path}"" {responseFileParameter}")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var process = Process.Start(startInfo);

            var stdout = new ProcessOutputStreamReader(process, process.StandardOutput);
            var stderr = new ProcessOutputStreamReader(process, process.StandardError);
            process.WaitForExit();
            var stdoutLines = stdout.GetOutput();
            var stderrLines = stderr.GetOutput();
            if (stderrLines.Length > 0)
            {
                AddWarning($"Internal Error running FindMissingDocs. Output:\n{string.Join("\n",stderrLines)}");
                return;
            }

            if (stdoutLines.Length > 0)
            {
                var errorMessage = FormatErrorMessage(stdoutLines);
                AddWarning(errorMessage);

                //// JonH: Enable errors in non-preview packages once the check has been put through its paces and the change is coordinated with RM and PM
                // if (Context.ProjectPackageInfo.IsPreview)
                //     Warning(errorMessage);
                // else
                // {
                //     TestState = TestState.Failed;
                //     Error(errorMessage);
                // }
            }

            if (responseFilePath != null)
                File.Delete(responseFilePath);
        }

        public static string FormatErrorMessage(IEnumerable<string> expectedMessages)
        {
            return $@"The following APIs are missing documentation: {string.Join(Environment.NewLine, expectedMessages)}";
        }
    }
}
