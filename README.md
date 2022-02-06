# Final OAuth .NET API

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/18e0bf7a5ae8420d989d62b287245f0a)](https://www.codacy.com/gh/gary-archer/oauth.apisample.netcore/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=gary-archer/oauth.apisample.netcore&amp;utm_campaign=Badge_Grade&x=1)

### Overview

The final OAuth secured .NET API code sample, referenced in my blog at https://authguidance.com:

- The API takes finer control over OAuth domain specific claims and uses a certified JOSE library
- The API also implements other [Non Functional Behaviour](https://authguidance.com/2017/10/08/corporate-code-sample-core-behavior/), for good technical quality

### Quick Start

Ensure that .NET 6 is installed, then run the start script to begin listening over HTTPS.\
You need to run the script at least once in order to download development SSL certificates.

- ./start.sh

### Details

* See the [Overview Page](http://authguidance.com/2018/01/05/net-core-code-sample-overview/) for further details on running the API
* See the [OAuth Integration Page](http://authguidance.com/2018/01/06/net-core-api-key-coding-points/) for key implementation details

### Programming Languages

* C# and .NET 6 are used to implement the REST API

### Middleware Used

* The Kestrel web server is used to host the API over SSL port 443
* AWS Cognito is used as the default Authorization Server
* The [jose-jwt Library](https://github.com/dvsekhvalnov/jose-jwt) is used to manage in memory validation of JWTs
* API logs can be aggregated to [Elasticsearch](https://authguidance.com/2019/07/19/log-aggregation-setup/) to support [Query Use Cases](https://authguidance.com/2019/08/02/intelligent-api-platform-analysis/)
