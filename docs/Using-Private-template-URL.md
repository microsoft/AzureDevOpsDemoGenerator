# Using Private template URL

Once you generate the custom tempalte using [Azure  DevOps Demo Generator - Template Builder](https://azuredevopsdemogenerator.azurewebsites.net/), you can use the same template as a master copy to provision the sample projects.

## Provisioning your project from your custom template using public URL

Using private template URL you can provisoin the project. You have multiple options here


1. Using the template URL from public GitHub repository, refer [Provisioning the project from your custom template](./Using-The-Template-Extractor.md)

1. Use the template URL in the home page of [Azure DevOps Demo Generator](https://azuredevopsdemogenerator.azurewebsites.net/) with querystring parameter **?templateurl**. This supports only the public URL
 
    >```Ex: https://azuredevopsdemogenerator.azurewebsites.net/?templateurl=public_url ```

    Once you format the URL, press enter to reload the page.
1. Click on **Sign In** button

1. Once you navigate to **Create Project** page, you should see the private tempalte selected by default

1. Select the **Organization**.  Provide the **Project Name**. Choose **Create Project** to start provisioning a project

    > **Note**: Once the process completes, the template will be discarded by the system. If you want to reuse the tempalte, you can use the **Private** option in the **Choose Template** dialog box

    You can refer the document [Provisioning your project from your custom template](./Using-The-Template-Extractor.md)

Previous: [Using the Extractor](./Using-The-Template-Extractor.md)
