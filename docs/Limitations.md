# Limitations of Azure DevOps Demo Generator

Azure DevOps Demo Generator is to create a demo set up with predefined templates, it has some limitations, which are listed below. This can't be treated as migration or copying project tool from one Organization to another.

## Following are the limitations of Azure DevOps Demo Generator and Template Builder

||Description|
|------|-----|
|Custom process template| Templates with Custom Process doesn't supports exporting Kanban board setting or the style. To use the project template generated from Custom Process or Inherited template, the target organization must have the same process and  template Id|
| Areas and Iterations| Not all the areas and iterations can be exported by the Template Builder with hierarchy. |
|Iteration dates| It has predefined logic to align the dates, this can't be changed based on the requirement |
| Work Items | Not all the work items will be exported. This count is limited to 200 work items of each type, if there are more than 200 work items |
|Work Item fields|Only few standard fields are supported, which are listed below. ```Title, State, Reason,Priority, Steps, Parameters, LocalDataSource, AutomationStatus, AcceptanceCriteria, Tags, RemainingWork, AssignedTo,  AreaPath, Description, Effort```|
| Wiki and Dashboard | These are not supported for General templates, we have some set of standart templates, which supports Wiki and Dashboard|
|Test Plans and Suites| Not supported for General templates, supported for standard templates which demonstrates the feature|
|Service connections|Service connections will be created based on the template requirement, those are needs to be authenticated by the user upon creating project|
|Source code| Source code can be present in public GitHub repository, which can be imported at the time of project creation, if the source code present in some Azure Repos, user has to provide PAT in ```/Template/ServiceEndPoints/**.json``` files at the time of importing the template, as shown below ```"username": "$username$","password": "$password$"``` |


