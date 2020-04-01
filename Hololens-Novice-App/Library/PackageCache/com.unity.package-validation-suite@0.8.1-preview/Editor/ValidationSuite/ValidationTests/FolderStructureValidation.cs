using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.PackageManager.ValidationSuite.ValidationTests;
using UnityEngine;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class FolderStructureValidation : BaseValidation
    {
        public FolderStructureValidation()
        {
            TestName = "Folder Structure Validation";
            TestDescription = "Verify that the folder structure meets expectations.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.CI, ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.Publishing, ValidationType.VerifiedSet };
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;
            List<string> problematicDirectoryList = new List<string>();

            ScanForResourcesDir(problematicDirectoryList, Context.PublishPackageInfo.path);

            if (problematicDirectoryList.Any())
            {
                AddWarning("The Resources Directory should not be used in packages.  For more guidance, please visit https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity6.html");
                problematicDirectoryList.ForEach(s => AddInformation("Problematic directory: /" + s));
            }
        }

        private void ScanForResourcesDir(List<string> problematicDirectoryList, string path)
        {
            var directories = Directory.GetDirectories(path);
            if (!directories.Any())
                return;

            foreach (var directory in directories)
            {
                ScanForResourcesDir(problematicDirectoryList, directory);
            }

            var resourcePath = Path.Combine(path, "Resources");
            if (Directory.Exists(resourcePath))
            {
                problematicDirectoryList.Add(resourcePath.Replace("\\", "/"));
            }
        }
    }
}
