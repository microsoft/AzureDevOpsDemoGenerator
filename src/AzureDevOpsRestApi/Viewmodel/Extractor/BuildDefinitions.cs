using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsAPI.Viewmodel.Extractor
{
    public class BuildDefinitions
    {
        public class Task
        {
            public string Id { get; set; }
            public string VersionSpec { get; set; }
            public string DefinitionType { get; set; }
        }

        public class Inputs
        {
            public object Command { get; set; }
            public object Arguments { get; set; }
            public object Cwd { get; set; }
            public string SourcePath { get; set; }
            public string FilePattern { get; set; }
            public object TokenRegex { get; set; }
            public object SecretTokens { get; set; }
            public object GulpFile { get; set; }
            public object Targets { get; set; }
            public object Gulpjs { get; set; }
            public object PublishJUnitResults { get; set; }
            public object TestResultsFiles { get; set; }
            public string TestRunTitle { get; set; }
            public object EnableCodeCoverage { get; set; }
            public object TestFramework { get; set; }
            public object SrcFiles { get; set; }
            public object TestFiles { get; set; }
            public string Platform { get; set; }
            public string Configuration { get; set; }
            public object Archs { get; set; }
            public object CordovaVersion { get; set; }
            public object AntBuild { get; set; }
            public object KeystoreFile { get; set; }
            public object KeystorePass { get; set; }
            public object KeystoreAlias { get; set; }
            public object KeyPass { get; set; }
            public object IosSignMethod { get; set; }
            public object IosSigningIdentity { get; set; }
            public object P12 { get; set; }
            public object P12Pwd { get; set; }
            public object UnlockDefaultKeychain { get; set; }
            public object DefaultKeychainPassword { get; set; }
            public object ProvProfileUuid { get; set; }
            public object ProvProfile { get; set; }
            public object RemoveProfile { get; set; }
            public object XcodeDeveloperDir { get; set; }
            public object WindowsAppx { get; set; }
            public object WindowsPhoneOnly { get; set; }
            public object WindowsOnly { get; set; }
            public object CordovaArgs { get; set; }
            public object OutputPattern { get; set; }
            public object TargetEmulator { get; set; }
            public object TestRunner { get; set; }
            public object MergeTestResults { get; set; }
            public string PublishRunAttachments { get; set; }
            public object ConnectedServiceName { get; set; }
            public object AppId { get; set; }
            public object BinaryPath { get; set; }
            public string SymbolsPath { get; set; }
            public object NativeLibraryPath { get; set; }
            public object NotesPath { get; set; }
            public object Notes { get; set; }
            public object Publish { get; set; }
            public object Mandatory { get; set; }
            public object Notify { get; set; }
            public object Tags { get; set; }
            public object Teams { get; set; }
            public object Users { get; set; }
            public string PathtoPublish { get; set; }
            public string ArtifactName { get; set; }
            public string ArtifactType { get; set; }
            public string TargetPath { get; set; }
            public string Solution { get; set; }
            public string MsbuildArgs { get; set; }
            public string Clean { get; set; }
            public string RestoreNugetPackages { get; set; }
            public string VsLocationMethod { get; set; }
            public string VsVersion { get; set; }
            public string VsLocation { get; set; }
            public string MsbuildLocationMethod { get; set; }
            public string MsbuildVersion { get; set; }
            public string MsbuildArchitecture { get; set; }
            public string MsbuildLocation { get; set; }
            public string LogProjectEvents { get; set; }
            public string TestAssembly { get; set; }
            public string TestFiltercriteria { get; set; }
            public string RunSettingsFile { get; set; }
            public string CodeCoverageEnabled { get; set; }
            public string OtherConsoleOptions { get; set; }
            public string VsTestVersion { get; set; }
            public string PathtoCustomTestAdapters { get; set; }
            public string CopyRoot { get; set; }
            public string Contents { get; set; }

        }
        public class Inputs1
        {
            public bool Parallel { get; set; }
            public bool ContinueOnError { get; set; }
            public string WorkItemType { get; set; }
            public bool AssignToRequestor { get; set; }
            public string AdditionalFields { get; set; }
            public string Multipliers { get; set; }

        }
        public class Build
        {
            public bool Enabled { get; set; }
            public bool ContinueOnError { get; set; }
            public bool AlwaysRun { get; set; }
            public string DisplayName { get; set; }
            public int TimeoutInMinutes { get; set; }
            public Task Task { get; set; }
            public Inputs Inputs { get; set; }
        }

        public class Definition
        {
            public string Id { get; set; }
        }

        public class Option
        {
            public bool Enabled { get; set; }
            public Definition Definition { get; set; }
            public Inputs1 Inputs { get; set; }
        }

        public class SystemDebug
        {
            public string Value = "";
            public bool AllowOverride = false;
        }

        public class BuildConfiguration
        {
            public string Value = "";
            public bool AllowOverride = false;
        }

        public class BuildPlatform
        {
            public string Value = "";
            public bool AllowOverride = false;
        }

        public class Variables
        {
            [JsonProperty(PropertyName = "system.debug")]
            public SystemDebug Systemdebug = new SystemDebug();
            public BuildConfiguration BuildConfiguration = new BuildConfiguration();
            public BuildPlatform BuildPlatform = new BuildPlatform();
        }

        public class RetentionRule
        {
            public IList<string> Branches { get; set; }
            public IList<object> Artifacts { get; set; }
            public IList<string> ArtifactTypesToDelete { get; set; }
            public int DaysToKeep { get; set; }
            public int MinimumToKeep { get; set; }
            public bool DeleteBuildRecord { get; set; }
            public bool DeleteTestResults { get; set; }
        }

        public class Properties
        {
            public string LabelSources { get; set; }
            public string ReportBuildStatus { get; set; }
        }

        public class Repository
        {
            public Properties Properties { get; set; }
            public string Id { get; set; }
            public string Type { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string DefaultBranch { get; set; }
            public string Clean { get; set; }
            public bool CheckoutSubmodules { get; set; }
        }

        public class Pool
        {
            public string Name { get; set; }
        }

        public class Queue
        {
            public Pool Pool { get; set; }
            public string Name { get; set; }
        }

        public class BuildDefinition
        {
            public string Name { get; set; }
            public IList<BuildDefinitions.Build> Build { get; set; }
            public IList<BuildDefinitions.Option> Options { get; set; }
            public Variables Variables { get; set; }
            public IList<RetentionRule> RetentionRules { get; set; }
            public string BuildNumberFormat { get; set; }
            public string JobAuthorizationScope { get; set; }
            public int JobTimeoutInMinutes { get; set; }
            public Repository Repository { get; set; }
            public string Quality { get; set; }
            public string DefaultBranch { get; set; }
            public Queue Queue { get; set; }
        }
    }
}
