# Proactive Bot Example

This example uses V3 of the bot framework and the teams channel connector library to send a message from a REST API call to a member of a Teams channel directly.

It uses the following nuget packages:
Microsoft.Bot.Builder version 3.8.0
Microsoft.Bot.Connector.Teams version 0.9.0

Inside the web.config be sure to set the following configuration values.

``` xml
  <appSettings>
    <!-- update these with your BotId, Microsoft App Id and your Microsoft App Password-->
    <add key="BotId" value="" />
    <add key="MicrosoftAppId" value="" />
    <add key="MicrosoftAppPassword" value="" />
    <add key="StorageAccount" value="UseDevelopmentStorage=true"/>
  </appSettings>
```

> BotId and MicrosoftAppId will likely be the same values (depending on how your app is registered). The StorageAccount setting is used to store data between conversationUpdate and our proactive message api endpoint.

