using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplatesGeneratorTool.ViewModel
{
    class BuildDefinitions
    {
        public class Task
        {
            public string id { get; set; }
            public string versionSpec { get; set; }
            public string definitionType { get; set; }
        }

        public class Inputs
        {
            public object command { get; set; }
            public object arguments { get; set; }
            public object cwd { get; set; }
            public string sourcePath { get; set; }
            public string filePattern { get; set; }
            public object tokenRegex { get; set; }
            public object secretTokens { get; set; }
            public object gulpFile { get; set; }
            public object targets { get; set; }
            public object gulpjs { get; set; }
            public object publishJUnitResults { get; set; }
            public object testResultsFiles { get; set; }
            public string testRunTitle { get; set; }
            public object enableCodeCoverage { get; set; }
            public object testFramework { get; set; }
            public object srcFiles { get; set; }
            public object testFiles { get; set; }
            public string platform { get; set; }
            public string configuration { get; set; }
            public object archs { get; set; }
            public object cordovaVersion { get; set; }
            public object antBuild { get; set; }
            public object keystoreFile { get; set; }
            public object keystorePass { get; set; }
            public object keystoreAlias { get; set; }
            public object keyPass { get; set; }
            public object iosSignMethod { get; set; }
            public object iosSigningIdentity { get; set; }
            public object p12 { get; set; }
            public object p12pwd { get; set; }
            public object unlockDefaultKeychain { get; set; }
            public object defaultKeychainPassword { get; set; }
            public object provProfileUuid { get; set; }
            public object provProfile { get; set; }
            public object removeProfile { get; set; }
            public object xcodeDeveloperDir { get; set; }
            public object windowsAppx { get; set; }
            public object windowsPhoneOnly { get; set; }
            public object windowsOnly { get; set; }
            public object cordovaArgs { get; set; }
            public object outputPattern { get; set; }
            public object targetEmulator { get; set; }
            public object testRunner { get; set; }
            public object mergeTestResults { get; set; }
            public string publishRunAttachments { get; set; }
            public object connectedServiceName { get; set; }
            public object appID { get; set; }
            public object binaryPath { get; set; }
            public string symbolsPath { get; set; }
            public object nativeLibraryPath { get; set; }
            public object notesPath { get; set; }
            public object notes { get; set; }
            public object publish { get; set; }
            public object mandatory { get; set; }
            public object notify { get; set; }
            public object tags { get; set; }
            public object teams { get; set; }
            public object users { get; set; }
            public string PathtoPublish { get; set; }
            public string ArtifactName { get; set; }
            public string ArtifactType { get; set; }
            public string TargetPath { get; set; }
            public string solution { get; set; }
            public string msbuildArgs { get; set; }
            public string clean { get; set; }
            public string restoreNugetPackages { get; set; }
            public string vsLocationMethod { get; set; }
            public string vsVersion { get; set; }
            public string vsLocation { get; set; }
            public string msbuildLocationMethod { get; set; }
            public string msbuildVersion { get; set; }
            public string msbuildArchitecture { get; set; }
            public string msbuildLocation { get; set; }
            public string logProjectEvents { get; set; }
            public string testAssembly { get; set; }
            public string testFiltercriteria { get; set; }
            public string runSettingsFile { get; set; }
            public string codeCoverageEnabled { get; set; }
            public string otherConsoleOptions { get; set; }
            public string vsTestVersion { get; set; }
            public string pathtoCustomTestAdapters { get; set; }
            public string CopyRoot { get; set; }
            public string Contents { get; set; }
         
        }
        public class Inputs1
        {
            public bool parallel { get; set; }
            public bool continueOnError { get; set; }
            public string workItemType { get; set; }
            public bool assignToRequestor { get; set; }
            public string additionalFields { get; set; }
            public string multipliers { get; set; }

        }
        public class Build
        {
            public bool enabled { get; set; }
            public bool continueOnError { get; set; }
            public bool alwaysRun { get; set; }
            public string displayName { get; set; }
            public int timeoutInMinutes { get; set; }
            public Task task { get; set; }
            public Inputs inputs { get; set; }
        }

        public class Definition
        {
            public string id { get; set; }
        }

        public class Option
        {
            public bool enabled { get; set; }
            public Definition definition { get; set; }
            public Inputs1 inputs { get; set; }
        }

        public class SystemDebug
        {
            public string value = "";
            public bool allowOverride = false;
        }

        public class BuildConfiguration
        {
            public string value = "";
            public bool allowOverride = false;
        }

        public class BuildPlatform
        {
            public string value = "";
            public bool allowOverride = false;
        }

        public class Variables
        {
            [JsonProperty(PropertyName = "system.debug")]
            public SystemDebug systemdebug = new SystemDebug();
            public BuildConfiguration BuildConfiguration = new BuildConfiguration();
            public BuildPlatform BuildPlatform = new BuildPlatform();
        }

        public class RetentionRule
        {
            public IList<string> branches { get; set; }
            public IList<object> artifacts { get; set; }
            public IList<string> artifactTypesToDelete { get; set; }
            public int daysToKeep { get; set; }
            public int minimumToKeep { get; set; }
            public bool deleteBuildRecord { get; set; }
            public bool deleteTestResults { get; set; }
        }

        public class Properties
        {
            public string labelSources { get; set; }
            public string reportBuildStatus { get; set; }
        }

        public class Repository
        {
            public Properties properties { get; set; }
            public string id { get; set; }
            public string type { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string defaultBranch { get; set; }
            public string clean { get; set; }
            public bool checkoutSubmodules { get; set; }
        }

        public class Pool
        {
            public string name { get; set; }
        }

        public class Queue
        {
            public Pool pool { get; set; }
            public string name { get; set; }
        }

        public class BuildDefinition
        {
            public string name { get; set; }
            public IList<TemplatesGeneratorTool.ViewModel.BuildDefinitions.Build> build { get; set; }
            public IList<TemplatesGeneratorTool.ViewModel.BuildDefinitions.Option> options { get; set; }
         
            public Variables variables { get; set; }
            public IList<RetentionRule> retentionRules { get; set; }
            public string buildNumberFormat { get; set; }
            public string jobAuthorizationScope { get; set; }
            public int jobTimeoutInMinutes { get; set; }
            public Repository repository { get; set; }
            public string quality { get; set; }
            public string defaultBranch { get; set; }
            public Queue queue { get; set; }
        }
    }
}
