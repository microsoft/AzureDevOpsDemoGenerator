# Contribute to the AzureDevOpsDemoGenerator

Welcome, and thank you for your interest in contributing to our Microsoft Cloud Workshop!

There are many ways that you can contribute, beyond writing or coding. The goal of this document is to provide a high-level overview of how you can get involved.

The Azure DevOps Demo Generator is a service which helps you provision projects. Now we have enabled feature to [build custom template](https://docs.microsoft.com/en-us/azure/devops/demo-gen/build-your-own-template?toc=/azure/devops/demo-gen/toc.json&bc=/azure/devops/demo-gen/breadcrumb/toc.json&view=azure-devops) and [provision your project](https://docs.microsoft.com/en-us/azure/devops/demo-gen/build-your-own-template?toc=/azure/devops/demo-gen/toc.json&bc=/azure/devops/demo-gen/breadcrumb/toc.json&view=azure-devops#provisioning-your-project-from-your-custom-template) using generated template, also you can share your template to community.

## How to contribute

Once you successfully generate and test your template, you can share it with community. This can be done in three ways.

1.  Create a pull request to [Azure DevOps Demo Generator](https://github.com/microsoft/AzureDevOpsDemoGenerator/)
1. Place the template zip file in any public repository and share the link in [Email](mailto:AzureDevOpsDemoGenerator@service.microsoft.com)
1. Share the template directly through [Email](mailto:AzureDevOpsDemoGenerator@service.microsoft.com)

    > If you have extensions installed in your organization, tempalte would be generated along with reference to all installed extensions in the ```Extensions.json``` file. You can edit this file and keep only the required extensions for the project. If extensions are not required, keep the file empty.

## Your template must contian following elements
|Keys|Description| 
|-------|-----------|
|**Name**| Name of your template, which will be displayed in the text box upon template selection |
|**ShortName** | Short name for your template. This should not contain any space or special characters|
|**TemplateFolder**| Name of your tempalte folder. This should not contain any space or special characters|
|**Description** | A brief description about the template|
|**Tags**| An array of tags, which should be related to technologies used. Ex: .NetCore, Java, Maven, Docker, K8S, etc. |
| **Icon** | Templte icon to display it publicly in the tempalte selection page |
| **Document link** | Document link for user reference|
|||

We will communicate via Email once the template validated successfully.


---

# Contributing to a Microsoft Cloud Workshop


## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Asking Questions

Have a question? Open an issue using the question template and the `question` label.  

The active community will be eager to assist you. Your well-worded question will serve as a resource to others searching for help.

## Providing Feedback

Your comments and feedback are welcome, and the project team is available via handful of different channels.

## Reporting Issues

Have you identified a reproducible problem in a workshop? Have a feature request? We want to hear about it! Here's how you can make reporting your issue as effective as possible.

### Identify Where to Report

The MSW project is distributed across multiple repositories. Try to file the issue against the correct workshop. Check the list of [Related Projects](https://github.com/Microsoft/MSW/wiki/Related-Projects) if you aren't sure which repo is correct.

### Look For an Existing Issue

Before you create a new issue, please do a search in [open issues](https://github.com/Microsoft/MSW/issues) to see if the issue or feature request has already been filed.

Be sure to scan through the [most popular](https://github.com/Microsoft/MSW/issues?q=is%3Aopen+is%3Aissue+label%3Afeature-request+sort%3Areactions-%2B1-desc) feature requests.

If you find your issue already exists, make relevant comments and add your [reaction](https://github.com/blog/2119-add-reactions-to-pull-requests-issues-and-comments). Use a reaction in place of a "+1" comment:

* 👍 - upvote
* 👎 - downvote


If you cannot find an existing issue that describes your bug or feature, create a new issue using the guidelines below.

### Writing Good Bug Reports and Feature Requests

File a single issue per problem and feature request. Do not enumerate multiple bugs or feature requests in the same issue.

Do not add your issue as a comment to an existing issue unless it's for the identical input. Many issues look similar, but have different causes.

The more information you can provide, the more likely someone will be successful reproducing the issue and finding a fix.  Please use the template for each issue.

### Final Checklist

Please remember to do the following:

* [ ] Search the issue repository to ensure your report is a new issue

* [ ] Recreate the issue after disabling all extensions

* [ ] Simplify your code around the issue to better isolate the problem

Don't feel bad if the developers can't reproduce the issue right away. They will simply ask for more information!

### Follow Your Issue

Once submitted, your report will go into the [issue tracking](https://github.com/Microsoft/vscode/wiki/Issue-Tracking) work flow. Be sure to understand what will happen next, so you know what to expect, and how to continue to assist throughout the process.

## Contributing Fixes

If you are interested in writing code to fix issues,
please see [How to Contribute](https://github.com/Microsoft/MSW/wiki/How-to-Contribute) in the wiki.

# Thank You!

Your contributions to open source, large or small, make great projects like this possible. Thank you for taking the time to contribute.
