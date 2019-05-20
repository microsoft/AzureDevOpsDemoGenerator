# Export and Create Dashboard 
Export the dashboard using template builder with valid placeholders
when importing/creating the dashboard, replace the placeholders with valid Ids

## Observations
- Dashboard contains different types of widgets
- Widgets can be segregated based on widget `typeId` 
- Widgets can contain `QueryId`,`BuildId`,`ReleaseId`,`GroupKey`,`RepositoryId` etc
- Not all the Ids can be replace with placeholders, only query Ids can be replaced. Since other widgets might have different names than the `Build Name`,`Release Name`, `Repo Name`