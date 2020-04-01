using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.ValidationSuite.ValidationTests;
using UnityEngine;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class UnityVersionValidation : BaseValidation
    {
        private string unityVersion;

        // Move code that validates that development is happening on the right version based on the package.json
        public UnityVersionValidation()
        {
            TestName = "Unity Version Validation";
            TestDescription = "Validate that the package was developed on the right version of Unity.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.CI };
        }

        // This method is called synchronously during initialization,
        // and allows a test to interact with APIs, which need to run from the main thread.
        public override void Setup()
        {
            unityVersion = UnityEngine.Application.unityVersion;
        }

        protected override void Run()
        {
            TestState = TestState.Succeeded;

            // Check Unity Version, make sure it's valid given current version of Unity
            double unityVersionNumber = 0;
            double packageUnityVersionNumber = 0;
            
            if (!double.TryParse(unityVersion.Substring(0, unityVersion.LastIndexOf(".")), out unityVersionNumber) ||
                (!string.IsNullOrEmpty(Context.ProjectPackageInfo.unity) && !double.TryParse(Context.ProjectPackageInfo.unity, out packageUnityVersionNumber)) ||
                unityVersionNumber < packageUnityVersionNumber)
            {
                AddError($"In package.json, \"unity\" is pointing to a version higher ({packageUnityVersionNumber}) than the editor you are currently using ({unityVersionNumber}). " +
                               $"Validation needs to happen on a version of the editor that is supported by the package.");
            }
        }
    }
}
