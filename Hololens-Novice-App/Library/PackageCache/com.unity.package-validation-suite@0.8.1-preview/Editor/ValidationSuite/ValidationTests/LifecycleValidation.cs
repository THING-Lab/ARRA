using System;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class LifecycleValidation : BaseValidation
    {
        public LifecycleValidation()
        {
            TestName = "Package Lifecycle Validation";
            TestDescription = "Validate that the pacakge respects the lifecycle transition guidelines.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.CI, ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.Publishing, ValidationType.VerifiedSet };
        }

        protected override void Run()
        {
            if (Context.PublishPackageInfo.lifecycle == 1.0)
            {
                TestState = TestState.Succeeded;
            }
            else
            {
                AddError(@"2020.2 Packages are not supported yet! We are working on transitioning to the package lifecycle version 2 in 2020.2, and the minimum required parts aren't ready.  Until further notice, please ensure the unity field in your package's package.json file is less than 2020.2.");
            }
        }
    }
}
