# Limitations of Azure DevOps Demo Generator

Azure DevOps Demo Generator is to create a demo set up with predefined templates, it has some limitations, which are listed below. This can't be treated as migration or copying project tool from one Organization to another.

## Following are the limitations of Azure DevOps Demo Generator and Template Builder

|||
|------|-----|
|Custom process template|This is supported to an extent, but not all the options available as compared to standard process template. Organization must contains same process template Id|
| Iterations| Not all the iterations can be exported by the Template Builder with hierarchy|
|Iteration dates| It has predefined logic to align the dates, this can't be changed based on the requirement |
| Work Items | Not all the work items will be exported. This count is limited to 200 work items of each type, if there are more than 200 work items |
|Work item fields|Only few standard fields are supported, ex: Title, state, iteration path, Description, Area Path |
| Wiki | Wiki will not be generated |
|Dashboards| Dashboards can't be generated, as it requires more of manual effort to configure widgets upon creation|
|Service connections|Service connections will be created based on the template requirement, those are needs to be authenticated by the user upon creating project|
|Source code| Source code can be present in public GitHub repository, which can be imported at the time of project creation, if the source code present in some Azure Repos, user has to provide PAT in ```/Template/ServiceEndPoints/**.json``` files at the time of importing the template, as shown below ```"username": "$username$","password": "$password$"``` |
|||

