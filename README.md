# Final OAuth .NET API

[![Codacy Badge](https://app.codacy.com/project/badge/Grade/18e0bf7a5ae8420d989d62b287245f0a)](https://www.codacy.com/gh/gary-archer/oauth.apisample.netcore/dashboard?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=gary-archer/oauth.apisample.netcore&amp;utm_campaign=Badge_Grade)

## Behaviour

The final OAuth secured .NET API code sample, referenced in my blog at https://authguidance.com:

- The API has a fictional business area of `investments`, but simply returns hard coded data
- The API takes finer control over OAuth domain specific claims and uses a certified JOSE library
- The API uses JSON request logging and Elasticsearch log aggregation, for measurability

### API integrates with UI Clients

The API can run as part of an OAuth end-to-end setup, to serve my blog's UI code samples.\
Running the API in this manner forces it to be consumer focused to its clients:

![SPA and API](./images/spa-and-api.png)

### API can be Productively Tested

The API's clients are UIs, which get user level access tokens by running an OpenID Connect code flow.\
For productive test driven development, the API instead mocks the Authorization Server:

![Test Driven Development](./images/tests.png)

### API can be Load Tested

A basic load test uses Tasks to fire 5 parallel requests at a time at the API.\
This ensures no concurrency problems, and error rehearsal is used, to ensure useful error responses:

![Load Test](./images/loadtest.png)

### API is Supportable

API logs can be analysed in use case based manner by running Elasticsearch SQL and Lucene queries.\
Follow the [Technical Support Queries](https://authguidance.com/2019/08/02/intelligent-api-platform-analysis/) for some people friendly examples:

![Support Queries](./images/support-queries.png)

## Commands

Ensure that .NET 7+ is installed, then run the API with this command:

```bash
./start.sh
```

### Configure DNS and SSL

Configure DNS by adding these domains to your hosts file:

```text
127.0.0.1 localhost apilocal.authsamples-dev.com login.authsamples-dev.com
```

Then call an endpoint over port 446:

```bash
curl -k https://apilocal.authsamples-dev.com:446/investments/companies
```

Configure [.NET SSL trust](https://authguidance.com/2017/11/11/developer-ssl-setup/#os-ssl-trust) for the root CA at `./certs/authsamples-dev.ca.pem`.

### Test the API

Stop the API, then re-run it with a test configuration:

```bash
./testsetup.sh
```

Then run integration tests and a load test:

```bash
./integration_tests.sh
./load_test.sh
```

## Further Details

* See the [Overview Page](https://authguidance.com/2018/01/05/net-core-code-sample-overview) for instructions on how to run the API
* See the [OAuth Integration Page](https://authguidance.com/2018/01/06/net-core-api-key-coding-points) for the security implementation
* See the [API Platform Design](https://authguidance.com/api-platform-design/) for details on scaling the design to many APIs

## Programming Languages

* C# and .NET 7 are used to implement the REST API

## Infrastructure

* The Kestrel web server is used to host the API over SSL
* AWS Cognito is used as the default Authorization Server
* The [jose-jwt Library](https://github.com/dvsekhvalnov/jose-jwt) is used to manage in memory validation of JWTs
* The project includes API deployment resources for Docker and Kubernetes
