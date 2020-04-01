using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using UnityEditor.PackageManager.ValidationSuite;
using UnityEngine;

namespace UnityEditor.PackageManager.ValidationSuite.ValidationTests
{
    internal class AssemblyDefinitionValidation : BaseValidation
    {
        private const string AssemblyFileDefinitionExtension = "*.asmdef";
        private const string CSharpScriptExtension = "*.cs";

        public AssemblyDefinitionValidation()
        {
            TestName = "Assembly Definition Validation";
            TestDescription = "Validate Presence and Contents of Assembly Definition Files.";
            TestCategory = TestCategory.ContentScan;
            SupportedValidations = new[] { ValidationType.CI, ValidationType.LocalDevelopment, ValidationType.LocalDevelopmentInternal, ValidationType.Publishing, ValidationType.VerifiedSet };
        }

        bool FindValueInArray(string[] array, string value)
        {
            var foundValue = false;
            for (int i = 0; i < array.Length && !foundValue; ++i)
            {
                foundValue = array[i] == value;
            }

            return foundValue;
        }

        void CheckAssemblyDefinitionContent(string assemblyDefinitionPath)
        {
            var simplifiedPath = assemblyDefinitionPath.Replace(Context.PublishPackageInfo.path, "{Package-Root}");
            var isInEditorFolder = assemblyDefinitionPath.IndexOf(Path.DirectorySeparatorChar+"Editor"+Path.DirectorySeparatorChar) >= 0;
            var isInTestFolder = assemblyDefinitionPath.IndexOf(Path.DirectorySeparatorChar+"Tests"+Path.DirectorySeparatorChar) >= 0;
            
            try
            {
                var assemblyDefinitionData = Utilities.GetDataFromJson<AssemblyDefinition>(assemblyDefinitionPath);
                var editorInIncludePlatforms = FindValueInArray(assemblyDefinitionData.includePlatforms, "Editor");

                var isTestAssembly = FindValueInArray(assemblyDefinitionData.optionalUnityReferences, "TestAssemblies") || FindValueInArray(assemblyDefinitionData.precompiledReferences, "nunit.framework.dll");
                
                // Assemblies in the Editor folder should not have any other platforms defined
                if (!isTestAssembly && isInEditorFolder && assemblyDefinitionData.includePlatforms.Length > 1)
                {
                    AddError("For editor assemblies, only 'Editor' should be present in 'includePlatform' in: [{0}]", simplifiedPath);
                }

                // Assemblies in the Editor folder must have Editor marked as platform
                if (!isTestAssembly && isInEditorFolder && !editorInIncludePlatforms)
                {
                    AddError("For editor assemblies, 'Editor' should be present in the includePlatform section in: [{0}]", simplifiedPath);
                }

                // Assemblies in the test folder must only be Test assemblies
                if (!isTestAssembly && isInTestFolder)
                {
                    AddError("Assembly {0} is not a test assembly and should not be present in the Tests folder of your package", simplifiedPath);
                }

            }
            catch (Exception e)
            {
                AddError("Can't read assembly definition {0}: {1}", simplifiedPath, e.Message);
            }
        }

        protected override void Run()
        {
            // Start by declaring victory
            TestState = TestState.Succeeded;
            var packagePath = Context.PublishPackageInfo.path;
            var isValidationSuite = Context.PublishPackageInfo.name == "com.unity.package-validation-suite";
            var manifestFilePath = Path.Combine(packagePath, Utilities.PackageJsonFilename);

            if (!File.Exists(manifestFilePath))
            {
                AddError("Can't find manifest: " + manifestFilePath);
                return;
            }

            // filter out `ApiValidationTestAssemblies` folder as the content of the folder is for testing only.
            Func<string, bool> filterTestAssemblies = f => !(isValidationSuite && f.IndexOf("ApiValidationTestAssemblies") >= 0);

            var asmdefFiles = Directory.GetFiles(packagePath, AssemblyFileDefinitionExtension, SearchOption.AllDirectories).Where(filterTestAssemblies);

            // check the existence of valid asmdef file if there are c# scripts in the Editor or Tests folder
            var foldersToCheck = new string[] {"Editor", "Tests"};
            foreach (var folder in foldersToCheck)
            {
                var folderPath = Path.Combine(packagePath, folder);
                if (!Directory.Exists(folderPath))
                    continue;

                var foldersWithAsmdefFile = asmdefFiles.Where(f => f.IndexOf(folderPath) >= 0).Select(f => Path.GetDirectoryName(f));
                var csFiles = Directory.GetFiles(folderPath, CSharpScriptExtension, SearchOption.AllDirectories).Where(filterTestAssemblies);
                foreach (var csFile in csFiles)
                {
                    // check if the cs file is not in any folder that has asmdef file
                    if (foldersWithAsmdefFile.All(f => csFile.IndexOf(f) < 0))
                    {
                        AddError("C# script found in \"" + folder + "\" folder, but no corresponding asmdef file: " + csFile);
                    }
                }
            }

            foreach (var asmdef in asmdefFiles)
                CheckAssemblyDefinitionContent(asmdef);
        }
    }
}
