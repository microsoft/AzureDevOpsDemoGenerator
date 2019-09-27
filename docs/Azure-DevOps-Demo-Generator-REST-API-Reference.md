# Welcome to the Azure DevOps Services REST API Reference
-------------------------
We have added API support to the Azure DevOps Demo Generator so that it can be invoked externally. This page has all the instructions that you need to know how to call the API.

* Calling the API from a [PowerShell Script](./Azure-DevOps-Demo-Generator-REST-API-Reference/Azure-DevOps-REST-API-%252D-Call-API-with-powershell.md)


## Create a Project

 `POST https://azuredevopsdemogenerator.azurewebsites.net/api/environment/create`

## Sample Request Body

```
{
	"accessToken": "********************************",
	"organizationName": "DemoProjects",
	"templateName": "contososhuttle2",
	"users": [		
		{
			"email": "abc@outlook.com",
			"ProjectName": "TestProject1"
		},
	{
			"email": "abc@outlook.com",
			"ProjectName": "TestProject2"
		}
    	]
}
```
### Parameters
<table>
  <tr>
    <th>Parameter</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>accessToken</td>
    <td>PAT for the Azure DevOps Org. Note that the projects will be created under the user name in which the PAT is created. The PAT should be created with the following scopes 
<li><b>read, write and manage </b> projects and teams and </li>
<li><b>read and write</b> permissions for Work Item, Code, Build, Wiki, Dashboard, Extensions, etc.,</li></td>
  </tr>
<tr>
<td> Organization name </td><td>Name of the Azure DevOps org in which the project will be created
</td>
</tr>
<tr><td> templatName</td><td>The short name of the template</td></tr>
<tr><td> users</td><td>Users information. You can specify any number of users.<br /><li>Email address of the user</li><li<>Name of the project </td></tr>
</table>

## Sample Response



## Validation Messages
<table>
<tr><td>Validation for account based on provided Organisation name</td><td>If Organisation Name is empty, then message will be "Provide a valid Account name" with status code 402 Bad Request</td></tr>
<tr><td>Validation for access token</td><td>If access token is empty, then message will be "Token of type Basic must be provided" with status code 402 Bad Request</td></tr>
<tr><td>Validation for Project Name</td><td>If the Project Name is invalid, then message will be "Invalid Project name" with status code 402 Bad Request
If the Project Name is same as reserved keywords, then message will be "Project name must not be a system-reserved name such as PRN, COM1, COM2, COM3, COM4, COM5, COM6, COM7, COM8, COM9, COM10, LPT1, LPT2, LPT3, LPT4, LPT5, LPT6, LPT7, LPT8, LPT9, NUL, CON, AUX, SERVER, SignalR, DefaultCollection, or Web" with status code 402 Bad request</td></tr>
<tr><td>Check for Duplicate Project Name in Request body</td><td>If the Project Name duplicate in the request body, then message will be "ProjectName must be unique" with status code 402 Bad Request</td></tr>
<tr><td>Validation for TemplateName</td><td>If templateName is empty, then message will be 
"Template Name should not be empty" with status code 402 Bad Request.
If the given template name not found in the source, then message will be "Template Not Found!" with status code 402 Bad Request</td></tr>
<tr><td>Validation for Email ID and Project Name</td><td>"EmailId or ProjectName is not found" with status code 402 Bad Request</td></tr>
</table>