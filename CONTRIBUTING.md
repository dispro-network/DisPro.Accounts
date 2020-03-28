# Contributing to Dispro.Accounts

_Thank you for taking the time to contribute to Dispro.Accounts!_ :raised_hands:

The following is a set of guidelines for contributors.

## Contents

[How Can I Contribute?](#how-can-i-contribute)

[Development](#development)

- [Overview](#development-overview)
- [Development Requirements](#development-development-requirements)
- [Getting Started](#development-getting-started)
- [Forks, Branches, and Pull Requests](#forks-branches-and-pull-requests)

[Additional Notes](#additional-notes)

## How Can I Contribute?

All contributions are welcome!

To report a problem or to suggest a new feature, [open a GitHub Issue](https://github.com/dispro-network/DisPro.Accounts/issues/new). This helps the maintainers to become aware of and prioritize work.

For code contributions (fixes or features), see [Development](#development)

## Development

### Overview

DisPro.Accounts is an OpenID Connect and OAuth 2.0 Identity Server based on [IdentityServer4](https://github.com/IdentityServer/IdentityServer4) built with the [.net core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0). The initial project files were created with the [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new) [`sso-sln template`](https://github.com/IdentitySolution/SSO-Solution).

Since `.net core` is cross-platform, development can happen on Windows, Linux and macOS. Currently the project has only been tested on Windows and so this guide is specific to a Windows development environment. If you're not on windows, please consider tackling these issues ([linux](#1), [macOS](#2)) to get the proper scripts and documentation added for all 3 platforms.

### Development Requirements

##### Windows

In order to develop DisPro.Accounts, you'll need:

- [.net core 3.0](https://dotnet.microsoft.com/download/dotnet-core/3.0)
- [openssl](https://tecadmin.net/install-openssl-on-windows/)
- [PostGreSQL](https://www.postgresql.org/download/windows/)

#### Linux, macOS

ToDo: [Linux](#1), [macOS](#2)

### Getting Started

#### 1. Clone the repository

##### Windows, Linux, macOS

Clone the repository and enter into it

```
git clone git@github.com:dispro-network/DisPro.Accounts.git
cd DisPro.Accounts
```

#### 2. Create self-signed SSL Certificates

##### Windows

Run the following script to create a wildcard (\*.dispro.network.local) self signed ssl certificates for development purposes. The certificates will be generated and placed in a folder named `certificates`. The password for the certificate is `SuperSecretPassword`.

The certificate will also be added as a trusted certificate to your machine.

```
cd Scripts
CallCreateSSLCertificate.cmd
```

##### Linux, macOS

ToDo: [Linux](#1), [macOS](#2)

#### 3. Add hosts to hosts file

In order to have clients and apis connect properly to DisPro.Accounts, local hosts will need to be added to your local hosts file. Running the following script will route calls from `accounts.dispro.network.local`, `api.dispro.network.local` and `dispro.network.local` to `127.0.0.1`.

##### Windows

```
CallCreateHosts.cmd
```

##### Linux, macOS

ToDo: [Linux](#1), [macOS](#2)

#### 4. Create appsettings.Development.json

Create an appsettings.Development.json file which is a .gitignored file that contains your development application settings. The file should contain the following. Be sure to update it with your own `DefaultConnection` connection string.

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "AllowedHosts": "accounts.dispro.network.local",
  "ConnectionStrings": {
    "DefaultConnection": "Your local PostGreSQL connection string"
  },
  "CertificateSettings": {
    "Filename": "dispro.network.local.pfx",
    "Password": "SuperSecretPasswor"
  }
}

```

#### 5. Initialize and Seed the Database

The database needs to be initialized and seeded for DisPro.Accounts to run properly. Navigate to `~\DisPro.Accounts\DisPro.Accounts` (where the `DisPro.Accounts.csproj` file is located) and run the following command:

```
dotnet run --seed --dont-run
```

**Notes**

- To clean the database run `dotnet run --clean`
- To clean and reseed the database run `dotnet run --clean --seed`
- To **only** migrate the database and not run the project add the `--dont-run` arg

##### 6. Run DisPro.Accounts

At this point the environment is fully setup, and you can run the server and login with one of the seeded usernames. From `~\DisPro.Accounts\DisPro.Accounts` run the following command:

```
dotnet run
```

Congratulations! You're now all set to make some amazing contributions! :tada:

### Forks, Branches, and Pull Requests

Community contributions to DisPro.Accounts require that you first fork this repository. After your modifications, push changes to your fork(s) and submit a pull request.

See GitHub documentation about [Collaborating with issues and pull requests](https://help.github.com/categories/collaborating-with-issues-and-pull-requests/) for more information.

> :exclamation: **Note:** _DisPro.Accounts development uses a long-lived `dev` branch for new (non-hotfix) development. Pull Requests should be opened against `dev` in all repositories._

#### Branching Model

DisPro.Accounts adheres to a branching model similar to Gitflow, a Git workflow designed around a strict branching model to more easily track feature development vs. releases. [For more information on Gitflow, check out Atlassian's helpful guide](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow).

The main difference being that there will not be release branches for every release. Production releases are tagged on the `master` branch. Final QA checks must occur on the `staging` branch before being released. This ensures that there is only a single live release at any given time.

We can separate our branches into long-lived and purposeful branches. We have two long-lived branches:

- **`master`**, checkout for hotfix development; this branch is the stable released version which the production site is using
- **`staging`**, you should never need to checkout this branch; this branch is for staging purposes before releasing to production
- **`dev`**, checkout for feature development; latest unstable releases and work targeting the next major or minor release.

All development is done on branches with a `prefix/title` style naming convention. These are later merged into `dev`, from here the release manager will merge the most stable commit into the `staging` branch. If all QA tests pass, the commit will be merged to `master` and tagged with a release.

- **`feature/`**, for new feature development; will be merged with `dev`.
- **`fix/`**, for minor fix development; will be merged with `dev`.
- **`hot-fix/`**, for hotfix development; will be merged with `master`, `staging` and `dev`.

For example, a new feature such as a new theme could be created as `feature/new-theme`

## Additional Notes

Join the chat in our [discord server](https://discord.gg/tGhJRcB). For general dev discussion relating to any DisPro project head over to the [dev-general](https://discord.gg/h6zvquc) channel. For dev discussion relating speficically to Dispro.Accounts, head over to [dev-dispro-accounts](https://discord.gg/eCRNMDd).

Thank you again for all your support, encouragement and effort! :heart:
