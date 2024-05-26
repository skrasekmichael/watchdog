# Watdog Anaylsis
Technical analysis according to [assignment](assignment.md).

## Architecture
The system needs to be horizontally scalable to be able to handle  high load (which also increases availability). Horizontal scaling implies that lookups for similar deals can't be done in memory.

### Components
- Load Balancer - distributes load to multiple instances of watchdog service
- Watchdog Service
  1) accepts HTTP requests from push API (endpoint per data source)
  2) temporary stores incoming data in certain format in storage
  3) performs analysis (i.e. similar deal analysis ...)
- Storage - provides distributed access to analyzed data

If there is a big disparity in load between different data sources, each data source (endpoint) cab be separated service for more granular horizontal scaling.

*Alternatively, serverless approach can be taken and model each HTTP endpoint as Azure Lambda Function.*

### Storage

Data characteristics:
- Immutable
- Append-to-Read ratio is 1:1, for 1 request
  - 1 deal is appended to storage
  - 1 analysis query is performed
- Analysis query filters by:
  - *timestamp*, *currency* (main reduction)
  - *VolumeToBalanceRatio* +- 5% tolerance
- Relevant data have are not older that 1 s +- delta time for analysis

Possible databases:
- **InfluxDB** (used in PoC)
  - Append-only, optimized for querying over **time ranges**
  - Easy integration/functional testing
  - open source
  - Some support for auto-removing old data (the lowest retention policy is only 1h)
- **CosmosDB**
  - More flexible
  - Data can be effectively sharded (*currency* as sharding key)
  - Support for auto-removing old data (TTL property in seconds)

### Service Components
- API - ASP.NET Core Web Api (endpoint per data source)
- App Common (API contracts, shared abstractions, notification abstractions ...)
- App Infrastrucre  - basic infrastructure configuration
- Modules/
  - Similar Deals
  - Similar Deals Infrastructure - integration of module to database
  - ...

#### Adding new data source
- create mapping functions for all analysis modules (API contract -> specific module analysis contract)
- create endpoint for data source, call analysis service from each module (and the mapper) 
- (extend tests to use added endpoint for creating data)

#### Adding new analysis module
- add assembly with logic and domain objects (analysis model, analysis service, mapper with mapping functions to the analysis model from all data sources)
- add infrastructure assembly with integration to database
- (add tests fot this analysis)

## Testing
Since main analysis is done using database-specific query language, units-tests are practically useless (there is nearly nothing to test).

### Function/Integration
Solution with InfluxDB can be easily tested in CI pipeline with test containers (see PoC). The CosmosDB may be more tricky to test (or run, based on my experience), so testing environment in Azure might be needed (could be reused for load testing).

### Load Testing
For load testing, the specific testing environment is needed, presumably in Azure, similar to production environment (or at least specific machine for consistent results).

## Deployment
CD to kubernetes (with configured horizontal scaling etc.) with some strategy for zero downtime when deploying (i.e. continuously forwarding traffic to instances with new application version and removing instances with older version)

## Monitoring
- Services should implement health check endpoint
- Application logs should be forwarded to Azure Insights
- Telemetry should be forwarded Azure Monitor

## Additional Clarifications
- domain expert needs to clarify:
  - concrete data needed in notifications for similar deal
  - how/where should the notifications be send
  - hom much delay from detection is not considered realtime detection (1s?/5s?/1min? ...)
- data sources clarification:
  - assignment states that:
    - configuration contains *"servers to connect to (multiple servers can be provided at once, different trading platforms in the future)"*
	- but also that *"DXtrade Push API will be the primary data source"*
  - is the configuration only for CORS configuration?
  - or are there multiple forms of data sources and the app has to connect to the servers to receive realtime data over some protocol?
  - or does the app only needs to do some login to these servers so that the servers start sending push notifications?
- ratio comparison clarification:
  - how exactly is the ration calculated (I am assuming $\frac{lot}{balance}$ or maybe $\frac{lot \cdot C}{balance}$)
  - the formula for comparing similar ratios:
   `|(deal1.VolumeToBalanceRatio - deal2.VolumeToBalanceRatio)| <= configuration.VolumeToBalanceRatio` IMO doesn't make sense:
    - if the ration of deal1 is less then 0.05 (assuming it is 5 % taken from the configuration) the ration of deal2 can ben near zero and it would mark them as same ratios (balance of deal2 can go to infinity and it wouldn't change the result of the formula) ... which doesn't seem right to me
    - it also doesn't work on data in provided examples (maybe I am wrongly calculating the ratio)
  - IMO, assuming I want to compare deal1 to other deals, the correct formula should be `|(deal1.VolumeToBalanceRatio - deal2.VolumeToBalanceRatio)| <= deal1.VolumeToBalanceRatio * configuration.Tolerance` (where tolerance is the mentioned 5 %) ... this works on provided examples (implemented in PoC)
- DXtrade Push API clarification:
  - exact model(s) received from the API ... I was not able to identify model that would be similar to provided examples in the API documentation
