# AzureRange
Generates Complementary Subnets of Azure Datacenter IP Ranges. Live site is at http://azurerange.azurewebsites.net

#Current Features
>- Creating a list of prefixes to advertise to Azure to mimic forced tunneling by excluding a set of IPs that are already accessible directly from inside the Microsoft Azure Cloud.
>- Selecting the regions of your choice to generate the custom list
>- Ability to choose which format (CSV, CIDR or prefix/mask, or IOS or ASA configuration samples).

#Future Plans
>- Ability to add custom IP prefixes for the customer to exclude specific ranges from the forced tunelling approach. For example, you might have an PaaS service or other web service that isn't part of Azure but for which you would still want Azure VNet resources to access that service without hairpinning back on premise. 

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
