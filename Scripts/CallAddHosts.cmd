@echo off
ECHO Running CallAddHosts

set hosts="accounts.dispro.network.local identity.accounts.dispro.network.local api.accounts.dispro.network.local mvc-implicit.accounts.dispro.network.local mvc-hybrid.accounts.dispro.network.local js.accounts.dispro.network.local react.accounts.dispro.network.local"
CD .\Scripts
.\AddHosts.cmd %hosts%