## Here is the details on how to call an API to create project using powershell

```
$pat = "_PAT_"
$encodedPat = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes(":$pat"))

$body = @"
{
    "accessToken": "_PAT_",
    "organizationName": "_ORG_",
    "templateName": "_TEMPLATENAME_",
    "users": [{
        "email":  "_EMAIL_",
        "ProjectName":  "_PROJECTNAME_"
    }]
}
"@

Write-Host "Provisioning project ..." -ForegroundColor Blue -BackgroundColor Cyan
$resp = Invoke-WebRequest -Uri "https://azuredevopsdemogenerator.azurewebsites.net/api/environment/create" -Method "POST" -ContentType application/json -Body $body

$returnCode = $resp.StatusCode
$returnStatus = $resp.StatusDescription

if ($returnCode -ne "202") {
    Write-Host "Create project failed - $returnCode $returnStatus" -ForegroundColor White -BackgroundColor Red
    break
}
Write-Host "Project queued ... awaiting completion ..." -ForegroundColor Blue -BackgroundColor Cyan

$method = "GET"
$listurl = "https://dev.azure.com/culater/_apis/projects?api-version=5.1-preview.4"
$resp = Invoke-RestMethod -Uri $listurl -Method $method -Headers @{Authorization = "Basic $encodedPat"}

#Wait till project is finished deploying
while (1 -eq 1)
{
    $resp = Invoke-RestMethod -Uri $listurl -Method $method -Headers @{Authorization = "Basic $encodedPat"}

    foreach ($project in $resp.value)
    {
        $projname = $project.name
        $projStatus = $project.state
        #Write-Host "Inspecting project $projname - $projStatus" -ForegroundColor Blue -BackgroundColor Cyan
        if ($projname -eq "davesvab-ContosoShuttle1" -and $projStatus -eq "wellFormed")
        {
            break 
        }
        Start-Sleep -seconds 1
    }

    if ($projname -eq "davesvab-ContosoShuttle1" -and $projStatus -eq "wellFormed")
    {
        break 
    }
}
Write-Host "Project provisioned successfully" -ForegroundColor Blue -BackgroundColor Cyan

```
