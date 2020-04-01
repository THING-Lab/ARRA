using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnityEditor.PackageManager.ValidationSuite
{
    public class TextReport
    {
        public string FilePath { get; set; }

        public TextReport(string packageId)
        {
            FilePath = ReportPath(packageId);
        }

        internal void Initialize(VettingContext context)
        {
            var packageInfo = context.ProjectPackageInfo;
            Write(
                string.Format("Validation Suite Results for package \"{0}\"\r\n", packageInfo.name) +
                string.Format(" - Path: {0}\r\n", packageInfo.path) +
                string.Format(" - Version: {0}\r\n", packageInfo.version) +
                string.Format(" - Lifecycle: {0}\r\n", packageInfo.lifecycle) +
                string.Format(" - Test Time: {0}\r\n", DateTime.Now) +
                string.Format(" - Tested with {0} version: {1}\r\n", context.VSuiteInfo.name, context.VSuiteInfo.version)
            );

            if (context.ProjectPackageInfo.dependencies.Any())
            {
                Append("\r\nPACKAGE DEPENDENCIES:\r\n");
                Append("--------------------\r\n");
                foreach (var dependencies in context.ProjectPackageInfo.dependencies)
                {
                    Append(string.Format("    - {0}@{1}\r\n", dependencies.Key, dependencies.Value));
                }
            }

            Append("\r\nVALIDATION RESULTS:\r\n");
            Append("-------------------\r\n");
        }

        public void Clear()
        {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
        }

        public void Write(string text)
        {
            File.WriteAllText(FilePath, text);
        }

        public void Append(string text)
        {
            File.AppendAllText(FilePath, text);
        }

        public void GenerateReport(ValidationSuite suite)
        {
            SaveTestResult(suite, TestState.Failed);
            SaveTestResult(suite, TestState.Succeeded);
            SaveTestResult(suite, TestState.NotRun);
            SaveTestResult(suite, TestState.NotImplementedYet);
        }

        void SaveTestResult(ValidationSuite suite, TestState testState)
        {
            foreach (var testResult in suite.ValidationTests.Where(t => t.TestState == testState))
            {
                Append(string.Format("\r\n{0} - \"{1}\"\r\n    ", testResult.TestState, testResult.TestName));
                if (testResult.TestOutput.Any())
                    Append(string.Join("\r\n\n    ", testResult.TestOutput.Select(o => o.ToString()).ToArray()) + "\r\n    ");
            }
        }

        public static string ReportPath(string packageId)
        {
            return Path.Combine(ValidationSuiteReport.ResultsPath, packageId + ".txt");
        }

        public static bool ReportExists(string packageId)
        {
            return File.Exists(ReportPath(packageId));
        }
    }
}
