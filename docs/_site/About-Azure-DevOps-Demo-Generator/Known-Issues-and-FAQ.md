[[_TOC_]]


# Known Issues:

## **Issue:** Error while creating release definition:
Tasks with versions 'ARM Outputs:4.*' are not valid for deploy job 'Agent job' in stage Stage 1

**Cause:** This is usually caused by one of the third-party extensions not enabled or installed in your Azure DevOps org. Usually installation of extensions are quick but sometimes, it  can take a few minutes (or even hours!) for an extension to be available to use, after it is installed in the marketplace. 

**Workaround:** You can try waiting for a few minutes and confirm whether the extension is available to use, and then run the generator again. 

------------------
# Frequently Asked Questions

##  Q: Is the Generator open-source? Can I get access to the code?

Azure DevOps Generator is not open-sourced yet. While we plan to make this open source, it is being evaluated by the product and legal teams

---------------

##  Q: How can I build my own template?

Yes, you can take a snapshot of your project and turn into a template and use it for provisioning future projects using the **Extractor** - See [Build-your-own-template](/About-Azure-DevOps-Demo-Generator/Build-your-own-template)

-----------