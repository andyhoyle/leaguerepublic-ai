# leaguerepublic-ai

Users can upload a photo of the match scorecard using this app. Leveraging Azure AI Foundry, the app will analyse the image, enter match score, submit the uploaded image as a match file, and enter statistics for each game including player name and the winner of each game. 

## Technology
This will use the latest stable version of .Net and use C# and Microsoft.Extensions.AI* NuGet packages to deliver the features. Infrastructure will be orchestrated using .NET Aspire.

## Architecture
There should be only one project but seperate concerns in folders for UI, API, Service (logic), Adapaters (for talking to Azure AI foundry and League republic website).
For the UI we're going to use Blazor and the component model so every UI element should be a component.
Allow the AI to be run by the test layer so put all application builder logic in a method which could be called by API or test project.

## Important
The League Republic API does not allow writing of data via the API so we'll have to ask users for their login and submit the score using the admin web interface. 

## Testing
Utilise Playwright to test League Republic Adapter.
Everything else should be tested using integration tests, and the API should be runnable using a test fixture.
