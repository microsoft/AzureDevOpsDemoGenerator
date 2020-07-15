# Frequently Asked Questions

Here are the few frequently asked questions or the knows issues

1. **Q: What are pre-requisites to use Azure DevOps Demo Generator?**<br>
**A:** You just need to have an organization with all the permissions required to create or manage the project.

1. **Q: Do we require organization prior to the using Azure DevOps Demo Generator?**<br>
**A:**   Yes, if you really want to create a project. Azure DevOps Demo Generator doesn't create organizations. User must have an organization before using the tool.

1. **Q: How much time does it take to create a project?** <br>
**A:** It depends on the template you have choosen. Roughly say 3-5 minutes.

1. **Q: My Organization is not listed under the picklist, what would be the reason?**<br>
**A:** You might have logged in with different **Azure Active Directory** other than the organization you are looking for. Please sign out and login after changing the directory. You can find reference guide to change the directory [here](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/change-azure-ad-connection?view=azure-devops)

1. **Q: Can we use this tool for project migration or to create a copy of the project?** <br>
**A:** This tool doesn't designed for migration or to copy the projects from one organization to another. This is only to provision a demo set up with predefined step by step instruction 

1. **Q: Can we fork the GitHub repository to any other GitHub organization?**<br>
**A:** Currently no. Repository will be forked to the organization associated to GitHub login id.

1. **Q: Info: Could not find any organizations, please change the directory and login** <br>
**A:** You might have logged in with different **Azure Active Directory** other than the organization you are looking for. Please sign out and login after changing the directory. You can find reference guide to change the directory [here](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/change-azure-ad-connection?view=azure-devops)

1. **Q: Error while creating release definition:** Tasks with versions ARM Outputs:4.* are not valid for deploy job Agent job in stage Stage 1 <br>
**A:** This is usually caused by one of the third-party extensions not enabled or installed in your Azure DevOps org. Usually installation of extensions are quick but sometimes, it can take a few minutes (or even hours!) for an extension to be available to use, after it is installed in the marketplace. <br>
You can try waiting for a few minutes and confirm whether the extension is available to use, and then run the generator again

1. **Q: Error while creating query: TF401256: You do not have Write permissions for query Shared Queries:** <br>
**A:** In Azure DevOps, users have different access levels - Basic, Stakeholder and Visual Studio Subscriber. Access levels determine what features are available to users. In order to provision projects using the demo generator, you need at least a Basic access level. This error indicates the user has a stakeholder license which does not grant permissions to writing shared queries. <br>
You should change the access level, from stakeholder to basic. Please refer to this article on docs: [Add users to your organization or project](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/add-organization-users?view=azure-devops) for more information on how to add users to your organization, and specify the level of features they can use.

1. **Q: TF50309: The following account does not have sufficient permissions to complete the operation** <br>
**A:** You do not have permissions to create new projects in the Azure DevOps organization you have selected. You will need to be a part of the Project Administrators group or have explicit permissions to create new projects. <br>
Please make sure you have the required permissions or try selecting a different Azure DevOps org where you project creation permission.

