using System;
using System.IO;
using System.Linq;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class LicenseValidation : BaseValidation
    {
        public LicenseValidation()
        {
            TestName = "License Validation";
            TestDescription = "Verify that licensing information is present, and filled out.";
            TestCategory = TestCategory.DataValidation;
            SupportedValidations = new[] { ValidationType.CI, ValidationType.LocalDevelopmentInternal, ValidationType.Publishing, ValidationType.VerifiedSet };
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            var licenseFilePath = Path.Combine(Context.PublishPackageInfo.path, Utilities.LicenseFile);
            var thirdPartyNoticeFilePath = Path.Combine(Context.PublishPackageInfo.path, Utilities.ThirdPartyNoticeFile);

            // Check that the package has a license.md file.  All packages should have one.
            if (File.Exists(licenseFilePath))
            {
                // TODO: If the license file exists, check that the copyright year is setup properly.
                CheckLicenseContent(licenseFilePath);
            }
            else
            {
                AddError("Every package must have a LICENSE.md file.");
            }

            // Check that the 3rd party notice file is not empty, and does not come from the starter kit.
            if (File.Exists(thirdPartyNoticeFilePath))
            {
                CheckThirdPartyNoticeContent(thirdPartyNoticeFilePath);

                // TODO: Signal to the vetting report that the package contains a 3rd party notice
            }
            else
            {
                // TODO: check that the code doesn't have any copyright headers if the 3rd party notice file is empty.
                CheckForCopyrightMaterial();
            }
        }

        protected void CheckLicenseContent(string licenseFilePath)
        {
            // if the file exists, make sure its not empty.
            var licenseContent = File.ReadAllLines(licenseFilePath);
            if (!licenseContent.Any())
            {
                AddError("A LICENSE.md file exists in the package, but it is empty.  All packages need a valid license");
                return;
            }

            // check that the license is valid.  We expect the first line to look like this:
            var expectedLicenseHeader1 = string.Format("{0} copyright \u00a9 {1} Unity Technologies ApS", Context.PublishPackageInfo.name, DateTime.UtcNow.Year);
            var expectedLicenseHeader2 = string.Format("{0} copyright \u00a9 {1} Unity Technologies ApS", Context.PublishPackageInfo.displayName, DateTime.UtcNow.Year);
            if (string.Compare(licenseContent[0], expectedLicenseHeader1, StringComparison.CurrentCultureIgnoreCase) != 0 &&
                string.Compare(licenseContent[0], expectedLicenseHeader2, StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                // TODO: Make this an error at some point soon.
                var message = string.Format("A LICENSE.md file exists in the package, but is in the wrong format.  " +
                                            "Ensure the copyright year is set properly, otherwise, please check the package starter kit's license file as reference.  " +
                                            "https://github.cds.internal.unity3d.com/unity/com.unity.package-validation-suite/blob/dev/LICENSE.md  " +
                                            "It was `{0}` but was expecting `{1}` or `{2}`",
                    licenseContent[0], expectedLicenseHeader1, expectedLicenseHeader2);
                AddWarning(message);
            }
        }

        protected void CheckThirdPartyNoticeContent(string filePath)
        {
            // if the file exists, make sure its not empty.
            var licenseContent = File.ReadAllLines(filePath);
            if (!licenseContent.Any())
            {
                AddError("A 3rd Party Notice file exists in the package, but it is empty.  If it isn't required, delete it, otherwise, follow this model to fill it out: https://github.cds.internal.unity3d.com/unity/com.unity.package-starter-kit/blob/master/Third%20Party%20Notices.md");
                return;
            }

            int numberOf3rdParties = 0;
            bool lookForLicenseType = false;
            for (int i = 0; i < licenseContent.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(licenseContent[i]))
                {
                    continue;
                }

                if (licenseContent[i].StartsWith("License Type:"))
                {
                    if (!lookForLicenseType)
                    {
                        AddError("Invalid 3rd Party Notice File.  Found License Type line without a previous Component Name line. Please follow the model outlined here: https://github.cds.internal.unity3d.com/unity/com.unity.package-starter-kit/blob/master/Third%20Party%20Notices.md" );
                        return;
                    }

                    lookForLicenseType = false;
                }

                if (licenseContent[i].StartsWith("Component Name:"))
                {
                    numberOf3rdParties++;

                    if (lookForLicenseType)
                    {
                        AddError("Invalid 3rd Party Notice File.  Found Component Name line, but no License Type line that follows. Please follow the model outlined here: https://github.cds.internal.unity3d.com/unity/com.unity.package-starter-kit/blob/master/Third%20Party%20Notices.md");
                        return;
                    }

                    lookForLicenseType = true;
                }
            }

            if (lookForLicenseType)
            {
                AddError("Invalid 3rd Party Notice File.  Found Component Name line, but no License Type line that follows. Please follow the model outlined here: https://github.cds.internal.unity3d.com/unity/com.unity.package-starter-kit/blob/master/Third%20Party%20Notices.md");
            }

            if (numberOf3rdParties == 0)
            {
                AddError("Invalid 3rd Party Notice File.  Found no valid entries. Please follow the model outlined here: https://github.cds.internal.unity3d.com/unity/com.unity.package-starter-kit/blob/master/Third%20Party%20Notices.md");
            }
        }

        protected void CheckForCopyrightMaterial()
        {
            
        }
    }
}
