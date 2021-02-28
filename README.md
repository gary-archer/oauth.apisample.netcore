# authguidance.apisample.netcore

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/18e0bf7a5ae8420d989d62b287245f0a)](https://www.codacy.com/gh/gary-archer/authguidance.apisample.netcore/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=gary-archer/authguidance.apisample.netcore&amp;utm_campaign=Badge_Grade&x=1)

### Overview

* The final API code sample using OAuth and Open Id Connect, referenced in my blog at https://authguidance.com

### Details

* See the [Overview Page](http://authguidance.com/2018/01/05/net-core-code-sample-overview/) for what the API does and how to run it
* See the [OAuth Integration Page](http://authguidance.com/2018/01/06/net-core-api-key-coding-points/) for details on OAuth Integration and Custom Claims handling

### Programming Languages

* C# and .Net 5 are used to implement the OAuth Secured REST API

### Middleware Used

* The Kestrel web server is used to host the API over SSL, using OpenSSL self signed certificates
* AWS Cognito is used as the Cloud Authorization Server
* The [IdentityModel Library](https://github.com/IdentityModel/IdentityModel) is used for API OAuth handling
* The [Microsoft In Memory Cache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory) is used to cache API claims in memory
* API logs can be aggregated to [Elastic Search](https://authguidance.com/2019/07/19/log-aggregation-setup/) to support [Query Use Cases](https://authguidance.com/2019/08/02/intelligent-api-platform-analysis/)