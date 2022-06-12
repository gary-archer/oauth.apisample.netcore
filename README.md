# Final OAuth .NET API

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/18e0bf7a5ae8420d989d62b287245f0a)](https://www.codacy.com/gh/gary-archer/oauth.apisample.netcore/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=gary-archer/oauth.apisample.netcore&amp;utm_campaign=Badge_Grade&x=1)

## Overview

The final OAuth secured .NET API code sample, referenced in my blog at https://authguidance.com:

- The API takes finer control over OAuth domain specific claims and uses a certified JOSE library
- The API uses JSON request logging and Elasticsearch log aggregation, for measurability

## API integrates with UI Clients

The API can run as part of an OAuth end-to-end setup, to serve my blog's UI code samples.\
Running the API in this manner forces it to be consumer focused to its clients:

![SPA and API](./doc/spa-and-api.png)

## API can be Productively Tested

The API's clients are UIs, which get user level access tokens by running an OpenID Connect code flow.\
For productive test driven development, the API instead mocks the Authorization Server:

![Test Driven Development](./doc/tests.png)

## API can be Load Tested

A basic load test uses Tasks to fire 5 parallel requests at a time at the API.\
This ensures no concurrency problems, and error rehearsal is used to verify that the API is supportable:

![Load Test](./doc/loadtest.png)

## Quick Start

Run the API in isolation with this command:

```bash
./start.sh
```

Then call an endpoint over port 446:

```bash
curl -k https://localhost:446/api/companies
```

Configure DNS by adding these domains to your hosts file:

```text
127.0.0.1 localhost api.authsamples-dev.com login.authsamples-dev.com
::1       localhost
```

Configure [.NET SSL trust](https://authguidance.com/2017/11/11/developer-ssl-setup/#os-ssl-trust) for the root CA at `./certs/authsamples-dev.ca.pem`.\
Then run API tests via these commands:

```bash
./testsetup.sh
./integration_tests.sh
./load_test.sh
```

## Further Details

* See the [Overview Page](http://authguidance.com/2018/01/05/net-core-code-sample-overview/) for instructions on how to run the API
* See the [OAuth Integration Page](http://authguidance.com/2018/01/06/net-core-api-key-coding-points/) for the security implementation
* See the [Non Functional Behaviour](https://authguidance.com/2017/10/08/corporate-code-sample-core-behavior/) page for a summary of overall qualities

## Programming Languages

* C# and .NET 6 are used to implement the REST API

## Infrastructure

* The Kestrel web server is used to host the API over SSL
* AWS Cognito is used as the default Authorization Server
* The [jose-jwt Library](https://github.com/dvsekhvalnov/jose-jwt) is used to manage in memory validation of JWTs
* API logs can be aggregated to [Elasticsearch](https://authguidance.com/2019/07/19/log-aggregation-setup/) to support [Query Use Cases](https://authguidance.com/2019/08/02/intelligent-api-platform-analysis/)
* The API is designed for [cloud native deployment](https://github.com/gary-archer/oauth.cloudnative.deployment) to Kubernetes
