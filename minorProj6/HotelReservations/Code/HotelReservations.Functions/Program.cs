using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;

// Azure Functions (isolated worker). Two stateless HTTP functions the business layer calls:
//   POST /api/quote    - itemized price for a stay (StayQuoteFunction)
//   POST /api/loyalty  - loyalty tier and points for a guest's history (LoyaltyFunction)
var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Build().Run();
