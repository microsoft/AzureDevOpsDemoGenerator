**Here is the details about how to track the project creation status using TrackId**

## Track project create status

 `GET https://azuredevopsdemogenerator.azurewebsites.net/api/environment/currentprogress?id={TrackId}`

### Parameters
<table>
  <tr>
    <th>Parameter</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>id</td>
    <td>id is TrackId value</td>
  </tr>
</table>

## Sample Response
```
"Project {ProjectName} created"
.
.
.
.
"Successfully Created"
```
