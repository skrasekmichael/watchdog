# c-sharp-dev-assignment
Provide technical analysis and propose application design for the described _Watchdog_ project. You can provide PoC implementation. The level of details and what you will focus on is up to you. If you are not familiar with any topic, feel free to skip it. The provided solution will be discussed with you.

What needs to be answered by your solution: 
* what yet needs to be clarified,
* what will be the application architecture,
* what services will be used (using Azure cloud),
* what will be the running costs,
* how will be the application tested,
* how will be the application deployed (and updated),
* how will be the application scaled in and out,
* how will be the application monitored.

## Watchdog project
The application will monitor trading activity in real-time and will notify any suspicious behavior. In the future, there will be several modules for different monitoring activities and different data sources, so the application architecture should take this into account and provide corresponding level of abstractions.

### Similar deals monitoring module
Trading servers can have some restrictions, toxic users can try to bypass these restrictions by using multiple accounts with different names (of some relatives for example) and on multiple servers. We need to detect such a behavior in real-time and check similar deals. 
Similar deals are deals where: 
* open time differs no more than one second, 
* currency pair is the same and 
* the difference in volume-to-balance ratio is no more than 5%. 

If such deals are detected produce notification with information including accounts and servers where the deals were detected. 

#### Application configuration 

Configuration contains the following parameters: 
* open time delta,
* trade volume to balance ratio,
* servers to connect to (multiple servers can be provided at once, different trading platforms in the future).

#### Implementation notes and requirements 

* [DXtrade Push API](https://demo.dx.trade/developers/#/DXtrade-Push-API) will be the primary data source
* The module should be designed so that it can be easily maintained, enhanced and tested (or its parts)
* There can be a lot of real-time data incoming in a short period of time so it's necessary to block the server callback for the least amount of time possible
* Publicly available libraries should be used reasonably
* In all cases only BUY/SELL deals are relevant
* Application should be able to compare data from multiple servers (cross-server detection of similar deals is necessary)
* Incoming trade records has to be compared in one-to-many principle (each deal has to be compared to all relevant incoming deals based on the configured parameters)
* Volume to balance ratio is ratio between the deal volume and current user balance  
  ```|(deal1.VolumeToBalanceRatio - deal2.VolumeToBalanceRatio)| <= configuration.VolumeToBalanceRatio```
* Deal IDs (Position) and user logins are unique only within a particular server

#### Examples
**Example 1**  
List of incoming deals:  
**Deal #1**, Balance 10 000, Buy EURUSD 1 lot at 2019-05-12 14:43:12  
**Deal #2**, Balance 10 000, Sell GBPUSD 0.2 lots at 2019-05-12 14:43:23  
**Deal #3**, Balance 10 000, Sell GBPUSD 0.21 lot at 2019-05-12 14:43:24 _<- triggered match with deal #2_  

**Example 2**  
List of incoming deals:  
**Deal #1**, Balance 10 000, Buy EURUSD 1 lot at 2019-05-12 14:43:12  
**Deal #2**, Balance 10 000, Sell GBPUSD 0.2 lots at 2019-05-12 14:43:23  
**Deal #3**, Balance 1 000, Sell GBPUSD 1.2 lots at 2019-05-12 14:43:23  
**Deal #4**, Balance 10 000, Sell GBPUSD 0.21 lot at 2019-05-12 14:43:24 _<- triggered match with deal #2_  
**Deal #5**, Balance 20 000, Sell GBPUSD 0.4 lot at 2019-05-12 14:43:24 _<- triggered match with deal #2 and deal #4_  

