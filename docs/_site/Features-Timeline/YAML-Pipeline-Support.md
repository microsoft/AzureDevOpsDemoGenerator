# Support for YAML pipeline
Sprint#32  May 6, 2019

------------------------------

In this sprint we are adding YAML pipelines to the generator. Templates can now have CI and CD definitions defined in YAML and those pipelines will be provisioned when creating a new instance through the generator. We are also adding to the extractor so users can extract projects that has pipelines defined in YAML. 

For more information on YAML pipelines in Azure Pipelines, see [https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema](https://docs.microsoft.com/en-us/azure/devops/pipelines/yaml-schema?view=azure-devops&tabs=schema)